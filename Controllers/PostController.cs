using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
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

        [HttpPatch]
        [Route("{post_id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        [Authorize]
        public Post EditPost([FromRoute(Name = "post_id")] int postId, EditPost editPost)
        {
            var user = GetAuthorizedUser();
            var post = GetPost(postId);
            if (!post.IsAuthor(user))
            {
                throw new StatusException(StatusCodes.Status403Forbidden,
                    $"User '{user.Username}' lacks permission to edit post '{post.Id}'");
            }

            if (editPost.IsFieldSet(nameof(editPost.Title)))
            {
                if (editPost.Title == null)
                {
                    throw new StatusException(StatusCodes.Status422UnprocessableEntity,
                        "Failed to validate field title: can't be null");
                }

                post.Title = editPost.Title;
            }

            if (editPost.IsFieldSet(nameof(editPost.Content)))
                post.Content = editPost.Content;
            if (editPost.IsFieldSet(nameof(editPost.Url)))
                post.Url = editPost.Url;
            post.Edited = DateTime.UtcNow;

            Db.SaveChanges();
            return post;
        }

        [HttpDelete]
        [Route("{post_id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [Authorize]
        public ActionResult DeletePost([FromRoute(Name = "post_id")] int postId)
        {
            var user = GetAuthorizedUser();
            var post = GetPost(postId);
            if (!post.CanDelete(user))
            {
                throw new StatusException(StatusCodes.Status403Forbidden,
                    $"User '{user.Username}' lacks permission to delete post '{post.Id}'");
            }

            Db.Remove(post);
            Db.SaveChanges();
            return NoContent();
        }
    }
}