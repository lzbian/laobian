using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Laobian.Common.Base;
using Laobian.Jarvis.Model;
using Laobian.Jarvis.Post;

namespace Laobian.Jarvis.Option.Post
{
    [Verb("new-post", HelpText = "Create new post template.")]
    public class NewPostOptions : Options
    {
        private readonly PostManager _postManager;

        public NewPostOptions()
        {
            _postManager = new PostManager();
        }

        [Option('n', "number", Default = 1, HelpText = "Specify number of new posts.")]
        public int Number { get; set; }

        [Value(0, HelpText = "Location of created posts")]
        public string TargetDirectory { get; set; }

        protected override async Task HandleInternalAsync()
        {
            var directory = Directory.GetCurrentDirectory();
            if (!string.IsNullOrEmpty(TargetDirectory))
            {
                directory = Path.GetFullPath(TargetDirectory);
            }

            if (!Directory.Exists(directory))
            {
                await JarvisOut.ErrorAsync($"Directory invalid or not exist: {directory}");
            }

            if (Number < 1)
            {
                Number = 1;
            }

            await JarvisOut.VerbAsync($"Attempt to generate new post at: {directory}, count: {Number}");

            for (var i = 0; i < Number; i++)
            {
                var post = new Common.Blog.BlogPost();
                post.Raw.Id = Guid.NewGuid().Normal();
                post.Raw.Title = $"<Title Placeholder - {Guid.NewGuid().Normal()}>";
                post.Raw.Category = $"<Category Placeholder - {Guid.NewGuid().Normal()}>";
                post.Raw.Url = $"Url-Placeholder-{Guid.NewGuid().Normal()}";
                post.Raw.PublishTime = DateTime.UtcNow.ToDateAndTime();
                post.Raw.Publish = bool.FalseString;
                post.Raw.Excerpt = $"<Excerpt Placeholder(Markdown) - {Guid.NewGuid().Normal()}>";
                post.Raw.Content = $"<Content Placeholder(Markdown) - {Guid.NewGuid().Normal()}>";

                var content = PostParser.ToRawData(post);
                var savedPath = Path.Combine(directory, _postManager.GetPostFileName(post.Raw.Id));
                await File.WriteAllTextAsync(savedPath, content, Encoding.UTF8);
                await JarvisOut.InfoAsync($"New post template created: {savedPath}");
            }
        }
    }
}
