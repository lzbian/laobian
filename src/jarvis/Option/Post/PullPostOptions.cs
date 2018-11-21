using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using Laobian.Common.Base;
using Laobian.Jarvis.Model;
using Laobian.Jarvis.Post;

namespace Laobian.Jarvis.Option.Post
{
    [Verb("pull-post", HelpText = "Pull remote post to local.")]
    public class PullPostOptions : Options
    {
        private readonly PostManager _postManager;

        public PullPostOptions()
        {
            _postManager = new PostManager();
        }

        [Option('t', "title", HelpText = "Remote post title.")]
        public string Title { get; set; }

        [Option('u', "url", HelpText = "Remote post URL.")]
        public string Url { get; set; }

        [Option('i', "id", HelpText = "Remote post ID.")]
        public string Id { get; set; }

        [Value(0, HelpText = "Location of pulled to.")]
        public string Location { get; set; }

        protected override async Task HandleInternalAsync()
        {
            var directory = Environment.CurrentDirectory;
            if (!string.IsNullOrEmpty(Location))
            {
                directory = Path.GetFullPath(Location);
            }

            if (!Directory.Exists(directory))
            {
                await JarvisOut.ErrorAsync($"Invalid path or not a directory: {directory}");
                return;
            }

            await JarvisOut.InfoAsync($"Attempt to pull blogPost to: {directory}");

            Common.Blog.BlogPost blogPost;
            var posts = await _postManager.GetAllPostsAsync();
            if (!string.IsNullOrEmpty(Id))
            {
                // pull via Id
                await JarvisOut.VerbAsync($"Pull via ID: {Id}");
                if (!Guid.TryParse(Id, out var id))
                {
                    await JarvisOut.ErrorAsync($"Invalid blogPost id: {Id}");
                    return;
                }

                blogPost = posts.FirstOrDefault(ps => ps.Id == id);
                if (blogPost == null)
                {
                    await JarvisOut.ErrorAsync($"BlogPost doesn't exist with id: {Id}");
                    return;
                }

                await _postManager.SaveToLocalAsync(directory, blogPost);
                return;
            }

            if (!string.IsNullOrEmpty(Url))
            {
                // pull via URL
                await JarvisOut.VerbAsync($"Pull via URL: {Url}");
                blogPost = posts.FirstOrDefault(ps => ps.Url.EqualsIgnoreCase(Url));
                if (blogPost == null)
                {
                    await JarvisOut.ErrorAsync($"BlogPost doesn't exist with URL: {Url}");
                    return;
                }

                await _postManager.SaveToLocalAsync(directory, blogPost);
                return;
            }

            // pull via Title
            await JarvisOut.VerbAsync($"Pull via Title: {Title}");
            blogPost = posts.FirstOrDefault(ps => ps.Title.EqualsIgnoreCase(Title));
            if (blogPost == null)
            {
                await JarvisOut.ErrorAsync($"BlogPost doesn't exist with Title: {Title}");
                return;
            }

            await _postManager.SaveToLocalAsync(directory, blogPost);
        }

        
    }
}
