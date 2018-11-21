namespace Laobian.Common.Azure
{
    /// <summary>
    /// Centralized place for managing Azure Blob namings
    /// </summary>
    public class BlobNameProvider
    {
        /// <summary>
        /// Get the App Setting blob name
        /// </summary>
        /// <returns>Normalized string represents App Setting blob name</returns>
        public static string AppSettingBlob()
        {
            return Normalize("appsetting");
        }

        /// <summary>
        /// Get the Blog BlogPost blob name
        /// </summary>
        /// <returns>Normalized string represents Blog BlogPost blob name</returns>
        public static string PostBlob()
        {
            return "blogpost";
        }

        /// <summary>
        /// Normalize given name to accepted one
        /// </summary>
        /// <param name="name">The given name</param>
        /// <returns>Normalized string can accepted by Microsoft Azure Blob</returns>
        public static string Normalize(string name)
        {
            return name.ToLowerInvariant();
        }

        /// <summary>
        /// Normalize given container name to accepted one
        /// </summary>
        /// <para name="containerName">
        /// The given container
        /// </para>
        /// <returns>Normalized string can accepted by Microsoft Azure Blob</returns>
        public static string Normalize(BlobContainer containerName)
        {
            return Normalize(containerName.ToString());
        }
    }
}
