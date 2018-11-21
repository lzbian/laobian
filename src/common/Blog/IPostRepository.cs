using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Laobian.Common.Blog
{
    /// <summary>
    /// Repository for Blog BlogPost
    /// </summary>
    public interface IPostRepository
    {
        /// <summary>
        /// Get all posts
        /// </summary>
        /// <returns>List of <see cref="BlogPost"/></returns>
        Task<List<BlogPost>> GetPostsAsync();

        /// <summary>
        /// Add new blogPost
        /// </summary>
        /// <param name="blogPost">To be added blogPost</param>
        /// <returns>Id of new added blogPost</returns>
        Task<Guid> AddAsync(BlogPost blogPost);

        /// <summary>
        /// Update existing post
        /// </summary>
        /// <param name="blogPost">Updated post</param>
        /// <returns>Task</returns>
        Task UpdateAsync(BlogPost blogPost);

        void UpdatePostsCache(List<BlogPost> posts);
    }
}
