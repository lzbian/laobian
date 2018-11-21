using System;
using System.Collections.Generic;
using System.Linq;
using Markdig;
using Newtonsoft.Json;
using ProtoBuf;

namespace Laobian.Common.Blog
{
    /// <summary>
    /// Blog post model
    /// </summary>
    [ProtoContract]
    public class BlogPost
    {
        public BlogPost()
        {
            Raw = new PostRawData();
        }

        #region Properties

        /// <summary>
        /// Raw markdown, which will be converted to HTML in runtime
        /// </summary>>
        [JsonProperty("raw")]
        [ProtoMember(1)]
        public PostRawData Raw { get; }

        /// <summary>
        /// Id of post
        /// </summary>
        [JsonIgnore]
        public Guid Id => Guid.Parse(Raw.Id);

        /// <summary>
        /// HTML content of post
        /// </summary>
        [JsonIgnore]
        public string Content => Markdown.ToHtml(GetData<string>(Raw.Content));

        /// <summary>
        /// Publish time of post
        /// </summary>
        [JsonIgnore]
        public DateTime PublishTime => GetData<DateTime>(Raw.PublishTime);

        /// <summary>
        /// Indicates whether post is published,
        /// True if yes, otherwise no
        /// </summary>
        [JsonIgnore]
        public bool Publish => bool.Parse(Raw.Publish);

        /// <summary>
        /// HTML declaimer of post
        /// </summary>
        [JsonIgnore]
        public string Declaimer => Markdown.ToHtml(GetData<string>(Raw.Declaimer) ?? string.Empty);

        /// <summary>
        /// HTML excerpt of post
        /// </summary>
        [JsonIgnore]
        public string Excerpt => Markdown.ToHtml(GetData<string>(Raw.Excerpt));

        /// <summary>
        /// Update time of post
        /// </summary>
        [ProtoMember(2)]
        [JsonProperty("updateTime")]
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// Category of post
        /// </summary>
        [JsonIgnore]
        public string Category => Raw.Category;

        /// <summary>
        /// URL of post
        /// </summary>
        [JsonIgnore]
        public string Url => Raw.Url;

        /// <summary>
        /// Title of post
        /// </summary>
        [JsonIgnore]
        public string Title => Raw.Title;

        /// <summary>
        /// References of post
        /// </summary>
        [JsonIgnore]
        public List<string> Reference
        {
            get
            {
                var rs = new List<string>();
                if (Raw.Reference == null)
                {
                    return rs;
                }

                rs.AddRange(Raw.Reference.Select(r => Markdown.ToHtml(r)));
                return rs;
            }
        }

        /// <summary>
        /// Count of visit of this post,
        /// This is set at runtime, need to call <see cref="SetVisitCount"/> after instance initialized
        /// </summary>
        [JsonIgnore]
        public int VisitCount { get; private set; }

        #endregion

        /// <summary>
        /// Get full URL of post, e.g. /2018/10/hello-world.html
        /// </summary>
        /// <returns>Full URL string</returns>
        public string GetFullUrl()
        {
            return $"/{PublishTime.Year:0000}/{PublishTime.Month:00}/{Url}.html";
        }

        /// <summary>
        /// Set count of visit
        /// </summary>
        /// <param name="count">The count</param>
        public void SetVisitCount(int count)
        {
            VisitCount = count;
        }

        private T GetData<T>(string rawData)
        {
            if (string.IsNullOrEmpty(rawData))
            {
                return default;
            }

            return (T)Convert.ChangeType(rawData, typeof(T));
        }

        

        
    }
}
