using System.Threading.Tasks;
using CommandLine;
using Laobian.Jarvis.Model;
using Laobian.Jarvis.Post;

namespace Laobian.Jarvis.Option.Post
{
    [Verb("update-post", HelpText = "Update existing post.")]
    public class UpdatePostOptions : Options
    {
        private readonly PostManager _postManager;

        public UpdatePostOptions()
        {
            _postManager = new PostManager();
        }

        [Value(0, Required = true, HelpText = "Markdown file which will be updated to remote.")]
        public string MarkdownFile { get; set; }

        protected override async Task HandleInternalAsync()
        {
            var path = _postManager.GetFullPath(MarkdownFile);
            await JarvisOut.VerbAsync($"Attempt to update post at: {MarkdownFile}");

            var post = await _postManager.GetPostAsync(path);
            await JarvisOut.VerbAsync($"BlogPost is valid and parsed: {MarkdownFile}");
            await _postManager.UpdatePostAsync(post, path);
        }
    }
}
