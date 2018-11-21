using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Laobian.Common.Base;
using Laobian.Common.Blog;

namespace Laobian.Jarvis.Post
{
    /// <summary>
    /// Parse for blogPost between markdown and object
    /// </summary>
    public class PostParser
    {
        private const string MetadataSign = "---";
        private const char SeparateChar = ':';

        /// <summary>
        /// Parse markdown string to <see cref="BlogPost"/> object
        /// </summary>
        /// <param name="text">The given markdown string</param>
        /// <returns>Parsed instance of <see cref="BlogPost"/></returns>
        public static async Task<BlogPost> ToPostAsync(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException(nameof(text));
            }

            return await ExtractPostAsync(text);
        }

        /// <summary>
        /// Parse <see cref="BlogPost"/> object to markdown text
        /// </summary>
        /// <param name="blogPost">The given <see cref="BlogPost"/> instance</param>
        /// <returns>Parsed markdown text</returns>
        public static string ToRawData(BlogPost blogPost)
        {
            var sb = new StringBuilder();
            sb.AppendLine(MetadataSign);

            sb.AppendLine(GetMetadataLine(nameof(blogPost.Raw.Id), blogPost.Raw.Id));
            sb.AppendLine(GetMetadataLine(nameof(blogPost.Raw.Title), blogPost.Raw.Title));
            sb.AppendLine(GetMetadataLine(nameof(blogPost.Raw.Category), blogPost.Raw.Category));
            sb.AppendLine(GetMetadataLine(nameof(blogPost.Raw.PublishTime), blogPost.Raw.PublishTime));
            sb.AppendLine(GetMetadataLine(nameof(blogPost.Raw.Publish), blogPost.Raw.Publish));

            if (!string.IsNullOrEmpty(blogPost.Raw.Declaimer))
            {
                sb.AppendLine(GetMetadataLine(nameof(blogPost.Raw.Declaimer), blogPost.Raw.Declaimer));
            }

            sb.AppendLine(GetMetadataLine(nameof(blogPost.Raw.Excerpt), blogPost.Raw.Excerpt));
            sb.AppendLine(GetMetadataLine(nameof(blogPost.Raw.Url), blogPost.Raw.Url));

            if(blogPost.Raw.Reference != null && blogPost.Raw.Reference.Any())
            {
                foreach(var item in blogPost.Raw.Reference)
                {
                    sb.AppendLine($"{nameof(blogPost.Raw.Reference)}{SeparateChar} {item}");
                }
            }

            sb.AppendLine(MetadataSign);
            sb.AppendLine(blogPost.Raw.Content);
            return sb.ToString();
        }

        private static async Task<BlogPost> ExtractPostAsync(string text)
        {
            var post = new BlogPost();
            using (var sr = new StringReader(text))
            {
                var metadataContents = new List<string>();
                var remainingContents = new List<string>();
                var metadataStarted = false;
                var metadataEnded = false;
                string line;
                while ((line = await sr.ReadLineAsync()) != null)
                {
                    if (metadataEnded)
                    {
                        remainingContents.Add(line);
                        continue; // parse remaining contents once metadata line passed
                    }

                    if (line.StartsWith(MetadataSign))
                    {
                        if (metadataStarted)
                        {
                            // metadata contents end
                            metadataEnded = true;
                        }
                        else
                        {
                            metadataStarted = true;
                        }

                    }
                    else if (metadataStarted)
                    {
                        metadataContents.Add(line);
                    }
                }

                if (!metadataStarted)
                {
                    throw new PostParseException("Failed to find entry line of metadata.");
                }

                if (!metadataEnded)
                {
                    throw new PostParseException("Failed to find exit line of metadata.");
                }

                if (!metadataContents.Any())
                {
                    throw new PostParseException("No metadata found between entry line and exit line.");
                }

                ExtractPostDetail(post, metadataContents);

                remainingContents.RemoveAll(rc => rc.Trim().EqualsIgnoreCase(Environment.NewLine));
                if (!remainingContents.Any())
                {
                    throw new PostParseException("No content found.");
                }

                post.Raw.Content = string.Join(Environment.NewLine, remainingContents);
                return post;
            }
        }

        private static void ExtractPostDetail(BlogPost blogPost, List<string> metadataContents)
        {
            var metadata = new List<KeyValuePair<string, string>>();
            foreach (var content in metadataContents)
            {
                var indexOfSeparateChar = content.IndexOf(SeparateChar);
                if (indexOfSeparateChar <= 0)
                {
                    continue;
                }

                var key = content.Substring(0, indexOfSeparateChar).Trim();
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                var value = content.Substring(indexOfSeparateChar + 1).Trim();
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                metadata.Add(new KeyValuePair<string, string>(key, value));
            }

            blogPost.Raw.Id = GetPropertyValue(metadata, nameof(blogPost.Raw.Id), true);
            if (Guid.TryParse(blogPost.Raw.Id, out var result))
            {
                blogPost.Raw.Id = result.Normal(); // force legacy ID format to normalized
            }
            else
            {
                throw new PostParseException($"Invalid post id: {blogPost.Raw.Id}.");
            }

            blogPost.Raw.Title = GetPropertyValue(metadata, nameof(blogPost.Raw.Title), true);
            blogPost.Raw.Category = GetPropertyValue(metadata, nameof(blogPost.Raw.Category), true);

            var publishTime = GetPropertyValue(metadata, nameof(blogPost.Raw.PublishTime));
            blogPost.Raw.PublishTime = !DateTime.TryParse(publishTime, out _) ? DateTime.UtcNow.ToString(CultureInfo.InvariantCulture) : publishTime;

            var publish = GetPropertyValue(metadata, nameof(blogPost.Raw.Publish));
            blogPost.Raw.Publish = !bool.TryParse(publish, out _) ? bool.FalseString : publish;

            blogPost.Raw.Declaimer = GetPropertyValue(metadata, nameof(blogPost.Raw.Declaimer));
            blogPost.Raw.Excerpt = GetPropertyValue(metadata, nameof(blogPost.Raw.Excerpt), true);
            blogPost.Raw.Url = GetPropertyValue(metadata, nameof(blogPost.Raw.Url), true);

            var refs = GetPropertyValues(metadata, nameof(blogPost.Raw.Reference));
            if(refs.Any())
            {
                blogPost.Raw.Reference = new List<string>();
                blogPost.Raw.Reference.AddRange(refs.Select(r => r.Value));
            }
        }

        private static string GetPropertyValue(List<KeyValuePair<string, string>> metadata, string name, bool mustExist = false)
        {
            var lines = GetPropertyValues(metadata, name);
            if(!lines.Any())
            {
                if (mustExist)
                {
                    throw new PostParseException($"{name} is not specified.");
                }

                return string.Empty;
            }

            return lines.Last().Value;
        }

        private static List<KeyValuePair<string, string>> GetPropertyValues(List<KeyValuePair<string, string>> metadata, string name)
        {
            return metadata.Where(m => m.Key.EqualsIgnoreCase(name)).ToList();
        }

        private static string GetMetadataLine(string name, string value)
        {
            return $"{name}{SeparateChar} {value}";
        }
    }
}
