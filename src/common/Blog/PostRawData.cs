using System.Collections.Generic;
using Newtonsoft.Json;
using ProtoBuf;

namespace Laobian.Common.Blog
{

    /// <summary>
    /// Raw markdown data of post.
    /// this maps to raw data which is composed by single markdown file,
    /// store this so that we can pull in raw data from cloud to local
    /// </summary>
    [ProtoContract]
    public class PostRawData
    {
        /// <summary>
        /// Gets or sets id string of post
        /// </summary>
        [JsonProperty("id")]
        [ProtoMember(1)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets title string of post
        /// </summary>
        [JsonProperty("title")]
        [ProtoMember(2)]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets content string of post
        /// </summary>
        [JsonProperty("content")]
        [ProtoMember(3)]
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets category string of post
        /// </summary>
        [JsonProperty("category")]
        [ProtoMember(4)]
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets publish time string of post
        /// </summary>
        [JsonProperty("publishTime")]
        [ProtoMember(5)]
        public string PublishTime { get; set; }

        /// <summary>
        /// Gets or sets is publish string of post
        /// </summary>
        [JsonProperty("publish")]
        [ProtoMember(6)]
        public string Publish { get; set; }

        /// <summary>
        /// Gets or sets declaimer string of post
        /// </summary>
        [JsonProperty("declaimer")]
        [ProtoMember(7)]
        public string Declaimer { get; set; }

        /// <summary>
        /// Gets or sets excerpt string of post
        /// </summary>
        [JsonProperty("excerpt")]
        [ProtoMember(8)]
        public string Excerpt { get; set; }

        /// <summary>
        /// Gets or sets url string of post
        /// </summary>
        [JsonProperty("url")]
        [ProtoMember(9)]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets reference collection of post
        /// </summary>
        [JsonProperty("reference")]
        [ProtoMember(10)]
        public List<string> Reference { get; set; }
    }
}
