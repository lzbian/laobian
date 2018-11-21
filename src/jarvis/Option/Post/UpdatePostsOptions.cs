using System;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using Laobian.Jarvis.Model;
using Laobian.Jarvis.Post;

namespace Laobian.Jarvis.Option.Post
{
    [Verb("update-posts", HelpText = "Update posts.")]
    public class UpdatePostsOptions : Options
    {
        private readonly PostManager _postManager;

        public UpdatePostsOptions()
        {
            _postManager = new PostManager();
        }

        [Option('f', "force", Default = true, Required = false, HelpText = "Skip failed post and continue.")]
        public bool ContinueOnFailure { get; set; }

        [Value(0, HelpText = "Location under which all posts will be updated.")]
        public string TargetDirectory { get; set; }

        protected override async Task HandleInternalAsync()
        {
            var path = Environment.CurrentDirectory;
            if(!string.IsNullOrEmpty(TargetDirectory))
            {
                path = Path.GetFullPath(TargetDirectory);
            }

            if (!Directory.Exists(path))
            {
                await JarvisOut.ErrorAsync($"Invalid path or not a directory: {path}");
                return;
            }

            await JarvisOut.VerbAsync($"Attempt to update all posts at: {path}");

            foreach(var file in Directory.EnumerateFiles(path, $"*{_postManager.MarkdownExtension}"))
            {
                await JarvisOut.InfoAsync("----------");
                try
                {
                    var fullPath = _postManager.GetFullPath(file);
                    await JarvisOut.InfoAsync($"Attempt to update post at: {fullPath}");
                    var post = await _postManager.GetPostAsync(fullPath);
                    await JarvisOut.VerbAsync($"BlogPost is valid and parsed: {fullPath}");
                    await _postManager.UpdatePostAsync(post, fullPath);
                }
                catch(Exception ex)
                {
                    if (!ContinueOnFailure)
                    {
                        await JarvisOut.ErrorAsync("Terminated - ", ex);
                        return;
                    }

                    await JarvisOut.InfoAsync($"Skipped - {ex.Message}");
                }
                await JarvisOut.InfoAsync("----------");
            }
        }
    }
}
