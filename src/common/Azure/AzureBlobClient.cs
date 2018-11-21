using System;
using Laobian.Common.Base;
using Laobian.Common.Config;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Laobian.Common.Azure
{
    /// <summary>
    /// Implementation of <see cref="IAzureBlobClient"/>
    /// </summary>
    public class AzureBlobClient : IAzureBlobClient
    {
        private readonly ConcurrentDictionary<string, bool> _containerCheckingDict;

        public AzureBlobClient()
        {
            _containerCheckingDict = new ConcurrentDictionary<string, bool>();
        }

        #region Implemention of IAzureBlobClient

        public async Task<T> DownloadAsync<T>(BlobContainer container, string blobName)
        {
            var containerRef = await GetContainerReferenceAsync(container);
            var blob = containerRef.GetBlockBlobReference(BlobNameProvider.Normalize(blobName));

            if (!await blob.ExistsAsync())
            {
                return default; // we don't want to ProtoBuf.NET throws exception later
            }

            using (var ms = new MemoryStream())
            {
                await blob.DownloadToStreamAsync(ms);
                return SerializeHelper.FromProtobuf<T>(ms);
            }
        }

        public async Task<bool> ExistAsync(BlobContainer container, string blobName)
        {
            var containerRef = await GetContainerReferenceAsync(container);
            var blob = containerRef.GetBlockBlobReference(BlobNameProvider.Normalize(blobName));
            return await blob.ExistsAsync();
        }

        public async Task<IEnumerable<BlobData>> ListAsync(BlobContainer container, string prefix = null)
        {
            var containerRef = await GetContainerReferenceAsync(container);
            BlobContinuationToken blobContinuationToken = null;

            var result = new List<BlobData>();

            do
            {
                var results = await containerRef.ListBlobsSegmentedAsync(
                    prefix, 
                    true, 
                    BlobListingDetails.All, 
                    new int?(), 
                    blobContinuationToken, 
                    null, 
                    null);
                // Get the value of the continuation token returned by the listing call.
                blobContinuationToken = results.ContinuationToken;
                foreach (var item in results.Results.Cast<CloudBlockBlob>())
                {
                    var ms = new MemoryStream();
                    await item.DownloadToStreamAsync(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    result.Add(new BlobData
                    {
                        BlobName = item.Name,
                        PrimaryUri = item.StorageUri.PrimaryUri.AbsolutePath,
                        Stream = ms,
                        Size = item.Properties.Length,
                        Created = item.Properties.Created,
                        ContentMD5 = item.Properties.ContentMD5
                    });
                }
            } while (blobContinuationToken != null);

            return result;
        }

        public async Task<string> UploadAsync<T>(BlobContainer container, string blobName, T obj)
        {
            var containerRef = await GetContainerReferenceAsync(container);
            var blob = containerRef.GetBlockBlobReference(BlobNameProvider.Normalize(blobName));

            if (obj is Stream objAsStream) // this is already stream, we don't need to go to Protobuf
            {
                using (objAsStream)
                {
                    await blob.UploadFromStreamAsync(objAsStream);
                }
            }
            else
            {
                using (var stream = SerializeHelper.ToProtobuf(obj))
                {
                    await blob.UploadFromStreamAsync(stream);
                }
            }            

            blob.Properties.CacheControl = "max-age=360000"; // for public resource, cache can help performance
            await blob.SetPropertiesAsync();
            await BackupBlob(blobName, obj);

            return blob.StorageUri.PrimaryUri.AbsoluteUri;
        }

        #endregion

        private async Task BackupBlob<T>(string blobName, T obj)
        {
            if (obj is Stream)
            {
                return;
            }

            var containerRef = await GetContainerReferenceAsync(BlobContainer.Backup);
            var blob = containerRef.GetBlockBlobReference(BlobNameProvider.Normalize($"{blobName}/{DateTime.UtcNow.ToDateAndTimeLite()}"));
            var json = SerializeHelper.ToJson(obj, true);
            await blob.UploadTextAsync(json);
        }

        private async Task<CloudBlobContainer> GetContainerReferenceAsync(BlobContainer containerName)
        {
            var container = BlobNameProvider.Normalize(containerName);
            var storageAccount = CloudStorageAccount.Parse(AppConfig.Default.AzureStorageConnection);
            var client = storageAccount.CreateCloudBlobClient();
            var containerRef = client.GetContainerReference(container);
            await EnsureContainerExistsAsync(containerRef);
            return containerRef;
        }

        private async Task EnsureContainerExistsAsync(CloudBlobContainer container)
        {
            var containerChecked = _containerCheckingDict.GetOrAdd(container.Name, false);
            if (!containerChecked)
            {
                await CreateContainerAsync(container);
                _containerCheckingDict.TryUpdate(container.Name, true, false);
            }
        }

        private async Task CreateContainerAsync(CloudBlobContainer containerRef)
        {
            await containerRef.CreateIfNotExistsAsync();

            var permissions = new BlobContainerPermissions
            {
                PublicAccess = containerRef.Name == BlobNameProvider.Normalize(BlobContainer.Public) ? BlobContainerPublicAccessType.Container : BlobContainerPublicAccessType.Off
            };

            await containerRef.SetPermissionsAsync(permissions);
        }
    }
}
