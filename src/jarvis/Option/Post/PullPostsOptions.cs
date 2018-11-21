using System;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using Laobian.Jarvis.Model;
using Laobian.Jarvis.Post;

namespace Laobian.Jarvis.Option.Post
{
    [Verb("pull-posts", HelpText = "Pull all remote posts to local.")]
    public class PullPostsOptions : Options
    {
        private readonly PostManager _postManager;

        public PullPostsOptions()
        {
            _postManager = new PostManager();
        }

        [Value(0, HelpText = "Location to pulled to.")]
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

            await JarvisOut.InfoAsync($"Attempt to pull post to: {directory}");
            var posts = await _postManager.GetAllPostsAsync();
            await JarvisOut.VerbAsync($"Attempt to save {posts.Count} posts to local");
            foreach (var post in posts)
            {
                await JarvisOut.InfoAsync("----------");
                await _postManager.SaveToLocalAsync(directory, post);
                await JarvisOut.InfoAsync("----------");
            }
        }
    }
}
