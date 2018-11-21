using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using Laobian.Common.Azure;
using Laobian.Common.Base;
using Laobian.Jarvis.Model;

namespace Laobian.Jarvis.Option.General
{
    [Verb("file", HelpText = "Manage files.")]
    public class FileOptions : Options
    {
        private readonly IAzureBlobClient _azureClient;

        public FileOptions()
        {
            _azureClient = new AzureBlobClient();
        }

        [Option('d', "download", HelpText = "Download all files to local.")] public bool Download { get; set; }

        [Option('i', "info", HelpText = "Show information of remote files.")] public bool Info { get; set; }

        [Value(0, HelpText = "Target directory location")] public string Location { get; set; }

        protected override async Task HandleInternalAsync()
        {
            if (Download)
            {
                await JarvisOut.VerbAsync("Fall in download option.");
                await DownloadFilesAsync();
                return;
            }

            if (Info)
            {
                await JarvisOut.VerbAsync("Fall in info option.");

                var files = await ListFilesAsync();
                var filesCount = 0;
                var filesSize = 0L;
                foreach (var blobData in files)
                {
                    filesCount++;
                    filesSize += blobData.Size;
                    await JarvisOut.InfoAsync($"{blobData.BlobName}\t\t{FileSizeHelper.Format(filesSize)}\t{blobData.Created}");
                }

                await JarvisOut.InfoAsync($"{filesCount} files, {FileSizeHelper.Format(filesSize)} in total.");
                return;
            }

            await JarvisOut.VerbAsync("Fall in upload option.");
            await UploadFileAsync();
        }

        private async Task DownloadFilesAsync()
        {
            var downloadFolder = Environment.CurrentDirectory;

            if (!string.IsNullOrEmpty(Location))
            {
                downloadFolder = Path.GetFullPath(Location);
            }

            if (!Directory.Exists(downloadFolder))
            {
                await JarvisOut.ErrorAsync($"Invalid path or not a directory: {downloadFolder}");
            }

            await JarvisOut.VerbAsync($"Attempt to download all posts to: {downloadFolder}");

            var files = new List<BlobData>(await ListFilesAsync());
            await JarvisOut.InfoAsync($"Attempt to download {files.Count} posts");

            foreach (var f in files)
            {
                var path = Path.Combine(downloadFolder, f.BlobName);
                using (var fs = File.Create(path))
                using (f.Stream)
                {
                    f.Stream.Seek(0, SeekOrigin.Begin);
                    await f.Stream.CopyToAsync(fs);
                    await JarvisOut.InfoAsync("File downloaded: {0}", path);
                }
            }
            
            await JarvisOut.InfoAsync("All files are downloaded to locally: {0}", downloadFolder);
        }

        private async Task UploadFileAsync()
        {
            var path = Path.GetFullPath(Location);
            if (!File.Exists(path))
            {
                await JarvisOut.ErrorAsync($"File not exists: {path}");
                return;
            }

            await JarvisOut.InfoAsync($"Attempt to upload file(prompt if exists): {path}");

            if (await _azureClient.ExistAsync(BlobContainer.Public, Path.GetFileName(path)))
            {
                await JarvisOut.InfoAsync("We found {0} already exists, do you want to override or cancel? Input O(o)/Y(y) to override, otherwise cancel.", Path.GetFileName(path));
                var choice = Console.ReadKey();
                if (choice.Key != ConsoleKey.Y && choice.Key != ConsoleKey.O)
                {
                    await JarvisOut.InfoAsync("Cancelled by user.");
                    Environment.Exit(1);
                }
                else
                {
                    await JarvisOut.VerbAsync("You selected override.");
                }
            }

            using (var stream = File.Open(path, FileMode.Open))
            {
                var url = await _azureClient.UploadAsync(BlobContainer.Public, Path.GetFileName(path), stream);
                await JarvisOut.InfoAsync("File uploaded successfully: {0}", url);
            }
        }

        private async Task<IEnumerable<BlobData>> ListFilesAsync()
        {
            return await _azureClient.ListAsync(BlobContainer.Public);
        }
    }
}