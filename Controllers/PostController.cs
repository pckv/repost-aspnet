using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepostAspNet.Models;

namespace RepostAspNet.Controllers
{
    [Route("/api/posts")]
    public class PostController : ApiControllerBase
    {
        public PostController(DatabaseContext context) : base(context)
        {
        }

        /// <summary>Get Post</summary>
        /// <remarks>Get a specific post in a resub.</remarks>
        [HttpGet]
        [Route("{post_id}", Name = "GetPost")]
        [ProducesResponseType(typeof(Post), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public Post GetPost([FromRoute(Name = "post_id")] int postId)
        {
            var post = Db.Posts
                .Include(p => p.ParentResub)
                .Include(p => p.Author)
                .SingleOrDefault(p => p.Id == postId);

            if (post == null)
            {
                throw new StatusException(StatusCodes.Status404NotFound, $"Post '{postId}' not found");
            }

            return post;
        }
    }
}