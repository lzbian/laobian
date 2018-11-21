using System;
using System.IO;

namespace Laobian.Common.Azure
{
    /// <summary>
    /// Represents Azure Blob data stream along with metadata
    /// </summary>
    public class BlobData
    {
        /// <summary>
        /// Gets or sets the blob name
        /// </summary>
        public string BlobName { get; set; }

        /// <summary>
        /// Gets or sets the primary HTTP URI string
        /// </summary>
        public string PrimaryUri { get; set; }

        /// <summary>
        /// Gets or sets data stream
        /// </summary>
        public MemoryStream Stream { get; set; }

        /// <summary>
        /// Gets or sets data stream size in bytes
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets created timestamp
        /// </summary>
        public DateTimeOffset? Created { get; set; }

        /// <summary>
        /// Gets or sets MD5 of content
        /// </summary>
        public string ContentMD5 { get; set; }
    }
}
