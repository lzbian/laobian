using System.Threading.Tasks;
using CommandLine;
using Laobian.Jarvis.Option.General;
using Laobian.Jarvis.Option.Post;

namespace Laobian.Jarvis.Option
{
    public class OptionDispatcher
    {
        public static async Task ParseAsync(string[] args)
        {
            await Parser.Default
                .ParseArguments<InfoOptions, FileOptions, NewPostOptions, AddPostOptions, AddPostsOptions,
                    UpdatePostOptions, UpdatePostsOptions, PullPostOptions, PullPostsOptions, ConfigOptions>(args)
                .MapResult(
                    (InfoOptions opts) => ExecuteOptions(opts),
                    (FileOptions opts) => ExecuteOptions(opts),
                    (NewPostOptions opts) => ExecuteOptions(opts),
                    (AddPostOptions opts) => ExecuteOptions(opts),
                    (AddPostsOptions opts) => ExecuteOptions(opts),
                    (UpdatePostOptions opts) => ExecuteOptions(opts),
                    (UpdatePostsOptions opts) => ExecuteOptions(opts),
                    (PullPostOptions opts) => ExecuteOptions(opts),
                    (PullPostsOptions opts) => ExecuteOptions(opts),
                    (ConfigOptions opts) => ExecuteOptions(opts),
                    errs =>
                    {
                        //JarvisOut.ErrorAsync($"Argument parsing failed: {string.Join(",", errs.Select(es => es.Tag))}").Wait();
                        return Task.FromResult(1);
                    });
        }

        private static async Task<int> ExecuteOptions(Options opts)
        {
            await opts.HandleAsync();
            return 0;
        }
    }
}
