using System;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using Laobian.Jarvis.Model;
using Laobian.Jarvis.Post;

namespace Laobian.Jarvis.Option.Post
{
    [Verb("add-posts", HelpText = "Add all posts in specified directory.")]
    public class AddPostsOptions : Options
    {
        private readonly PostManager _postManager;

        public AddPostsOptions()
        {
            _postManager = new PostManager();
        }

        [Option('f', "force", Default = true, Required = false, HelpText = "Skip failed post and continue.")]
        public bool ContinueOnFailure { get; set; }

        [Value(0, HelpText = "Local posts directory.")]
        public string TargetDirectory { get; set; }

        protected override async Task HandleInternalAsync()
        {
            var directory = Environment.CurrentDirectory;
            if(!string.IsNullOrEmpty(TargetDirectory))
            {
                directory = Path.GetFullPath(TargetDirectory);
            }

            if (!Directory.Exists(directory))
            {
                await JarvisOut.ErrorAsync($"Invalid path or not a directory: {directory}");
                return;
            }

            await JarvisOut.VerbAsync($"Attempt to add all posts at: {directory}");

            foreach (var file in Directory.EnumerateFiles(directory, $"*{_postManager.MarkdownExtension}"))
            {
                await JarvisOut.InfoAsync("----------");
                try
                {
                    var fullPath = _postManager.GetFullPath(file);
                    await JarvisOut.InfoAsync($"Attempt to add post at: {fullPath}");
                    var post = await _postManager.GetPostAsync(fullPath);
                    await JarvisOut.VerbAsync($"BlogPost is valid and parsed: {fullPath}");
                    await _postManager.AddPostAsync(post, fullPath);
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
