using Laobian.Common.Azure;
using Laobian.Common.Base;
using Laobian.Common.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Laobian.Common.Setting;

namespace Laobian.Common.Blog
{
    /// <summary>
    /// Implementation of <see cref="IPostRepository"/>
    /// </summary>
    public class PostRepository : IPostRepository
    {
        private const string CacheKey = "LAOBIAN_POSTS";
        private readonly IAzureBlobClient _azureClient;
        private readonly ICacheClient _cacheClient;

        public PostRepository(IAzureBlobClient azureClient)
        {
            _azureClient = azureClient;
            _cacheClient = new MemoryCacheClient();
        }

        #region Implementation of IPostRepository

        public async Task<List<BlogPost>> GetPostsAsync()
        {
            if (!_cacheClient.TryGet(CacheKey, out List<BlogPost> posts))
            {
                posts = await _azureClient.DownloadAsync<List<BlogPost>>(
                    BlobContainer.Private,
                    BlobNameProvider.PostBlob());
                if (posts == null)
                {
                    return new List<BlogPost>();
                }

                foreach (var i in SystemState.PostsVisitCount)
                {
                    var post = posts.FirstOrDefault(ps => ps.Id == i.Key);
                    post?.SetVisitCount(i.Value);
                }

                UpdatePostsCache(posts);
            }

            return posts;
        }

        public async Task<Guid> AddAsync(BlogPost blogPost)
        {
            if (blogPost.PublishTime == default)
            {
                blogPost.Raw.PublishTime = DateTime.UtcNow.ToDateAndTime();
            }

            if (blogPost.Id == default)
            {
                blogPost.Raw.Id = Guid.NewGuid().Normal();
            }

            blogPost.UpdateTime = DateTime.UtcNow;
            var posts = await GetPostsAsync();
            if (posts.FirstOrDefault(p => p.Id == blogPost.Id) != null)
            {
                throw new InvalidOperationException($"BlogPost with ID({blogPost.Id}) already exists.");
            }

            ValidatePost(blogPost, posts);

            posts.Add(blogPost);
            await UploadPostsAsync(posts);
            return blogPost.Id;
        }

        public async Task UpdateAsync(BlogPost blogPost)
        {
            var posts = await GetPostsAsync();
            if (blogPost.Id == default)
            {
                throw new InvalidOperationException(
                    "This blogPost has no ID assigned, it might be not created yet, try add-blogPost command instead.");
            }

            var existingPost = posts.FirstOrDefault(p => p.Id == blogPost.Id);
            if (existingPost == null)
            {
                throw new InvalidOperationException($"BlogPost with ID({blogPost.Id}) does not exists.");
            }

            if (SerializeHelper.ToJson(blogPost) == SerializeHelper.ToJson(existingPost))
            {
                // there is no need to update, save the operation
                return;
            }

            posts.Remove(existingPost);
            ValidatePost(blogPost, posts);

            blogPost.SetVisitCount(existingPost.VisitCount);
            blogPost.UpdateTime = DateTime.UtcNow;
            posts.Add(blogPost);
            await UploadPostsAsync(posts);
        }

        #endregion

        private void UpdatePostsCache(List<BlogPost> posts)
        {
            if (posts == null)
            {
                posts = new List<BlogPost>();
            }

            _cacheClient.Set(CacheKey, posts, AppSetting.Default.BlogPostReloadInterval);
            SystemState.PostReloadedAt = DateTime.UtcNow;
            SystemState.PublishedPosts = posts.Count(p => p.Publish);
        }

        private void ValidatePost(BlogPost blogPost, IReadOnlyCollection<BlogPost> posts)
        {
            if (posts.FirstOrDefault(p => p != blogPost && p.Url.EqualsIgnoreCase(blogPost.Url)) != null)
            {
                throw new InvalidOperationException($"BlogPost with URL({blogPost.Url}) already exists.");
            }

            if (posts.FirstOrDefault(p => p!= blogPost && p.Title.EqualsIgnoreCase(blogPost.Url)) != null)
            {
                throw new InvalidOperationException($"BlogPost with Title({blogPost.Title}) already exists.");
            }
        }

        private async Task UploadPostsAsync(List<BlogPost> posts)
        {
            await _azureClient.UploadAsync(BlobContainer.Private, BlobNameProvider.PostBlob(), posts);
            _cacheClient.Remove(CacheKey);
        }
    }
}