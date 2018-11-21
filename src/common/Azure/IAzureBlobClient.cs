using System.Collections.Generic;
using System.Threading.Tasks;

namespace Laobian.Common.Azure
{
    /// <summary>
    /// Client for operations of Microsoft Azure Blob
    /// </summary>
    public interface IAzureBlobClient
    {
        /// <summary>
        /// Upload to azure blob
        /// </summary>
        /// <typeparam name="T">The uploaded object type</typeparam>
        /// <param name="container">The target container</param>
        /// <param name="blobName">The uploaded blob name</param>
        /// <param name="obj">The uploaded object. This should be attributed by <c>ProtoContract</c>
        /// as it will be serialized to ProtoBuf binary.</param>
        /// <returns>The HTTP address.</returns>
        Task<string> UploadAsync<T>(BlobContainer container, string blobName, T obj);

        /// <summary>
        /// Download requested blob
        /// </summary>
        /// <typeparam name="T">The returned object type.
        /// This should be attributed by <c>ProtoContract</c>, as it will be deserialized from ProtoBuf binary.
        /// </typeparam>
        /// <param name="container">The target container</param>
        /// <param name="blobName">The requested blob name</param>
        /// <returns>The requested object</returns>
        /// <remarks>If requested blob doesn't exist, default value of <c>T</c> will be returned</remarks>
        Task<T> DownloadAsync<T>(BlobContainer container, string blobName);

        /// <summary>
        /// List all blobs
        /// </summary>
        /// <param name="container">The target container</param>
        /// <param name="prefix">The requested prefix</param>
        /// <returns>Collection of <see cref="BlobData"/>.</returns>
        Task<IEnumerable<BlobData>> ListAsync(BlobContainer container, string prefix = null);

        /// <summary>
        /// Check requested blob exists or not
        /// </summary>
        /// <param name="container">The target container</param>
        /// <param name="blobName">The requested blob name</param>
        /// <returns>Return true if exists, otherwise false.</returns>
        Task<bool> ExistAsync(BlobContainer container, string blobName);
    }
}
