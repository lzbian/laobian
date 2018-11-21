using System.Threading.Tasks;
using CommandLine;
using Laobian.Jarvis.Model;
using Laobian.Jarvis.Post;

namespace Laobian.Jarvis.Option.Post
{
    [Verb("add-post", HelpText = "Add new post.")]
    public class AddPostOptions : Options
    {
        private readonly PostManager _postManager;

        public AddPostOptions()
        {
            _postManager = new PostManager();
        }

        [Value(0, Required = true, HelpText = "Target markdown file")]
        public string MarkdownFile { get; set; }

        protected override async Task HandleInternalAsync()
        {
            var path = _postManager.GetFullPath(MarkdownFile);
            await JarvisOut.VerbAsync($"Attempt to add new post at: {path}");

            var post = await _postManager.GetPostAsync(path);
            await JarvisOut.VerbAsync("BlogPost content is valid and parsed");
            await _postManager.AddPostAsync(post, path);
        }
    }
}
