using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public PostController(RepostContext context) : base(context)
        {
        }

        /// <summary>Get Post</summary>
        /// <remarks>Get a specific post in a resub.</remarks>
        [HttpGet]
        [Route("{post_id}", Name = "GetPost")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public Post GetPost([FromRoute(Name = "post_id")] int postId)
        {
            var post = Db.Posts
                .Include(p => p.ParentResub)
                .Include(p => p.Author)
                .Include(p => p.Votes)
                .SingleOrDefault(p => p.Id == postId);

            if (post == null)
            {
                throw new StatusException(StatusCodes.Status404NotFound, $"Post '{postId}' not found");
            }

            return post;
        }

        /// <summary>Edit Post</summary>
        /// <remarks>
        ///     Edit a post in a resub.
        ///     Only the author of a post can edit the post.
        /// </remarks>
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

        /// <summary>Delete Post</summary>
        /// <remarks>
        ///     Delete a post in a resub.
        ///     Only the author of a post or the owner of the parent resub can delete the post.
        /// </remarks>
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

        /// <summary>Vote Post</summary>
        /// <remarks>Vote on a post in a resub.</remarks>
        [HttpPatch]
        [Route("{post_id}/vote/{vote}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        [Authorize]
        public Post VotePost([FromRoute(Name = "post_id")] int postId, [Range(-1, 1)] int vote)
        {
            var author = GetAuthorizedUser();
            var post = GetPost(postId);

            var postVote = Db.PostsVotes.SingleOrDefault(p => p.User == author && p.Post == post);
            if (postVote != null)
            {
                if (vote == 0)
                {
                    Db.PostsVotes.Remove(postVote);
                }
                else
                {
                    postVote.Vote = vote;
                }
            }
            else
            {
                Db.PostsVotes.Add(new PostVote
                {
                    User = author,
                    Post = post,
                    Vote = vote
                });
            }

            Db.SaveChanges();
            Db.Entry(post).Reload();
            return post;
        }

        /// <summary>Get Comments In Post</summary>
        /// <remarks>Get all posts in a resub.</remarks>
        [HttpGet]
        [Route("{post_id}/comments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public IEnumerable<Comment> GetCommentsInPost([FromRoute(Name = "post_id")] int postId, int page = 0,
            [FromQuery(Name = "page_size")] int pageSize = 100)
        {
            var post = GetPost(postId);
            Db.Entry(post)
                .Collection(p => p.Comments).Query()
                .Skip(page * pageSize)
                .Take(pageSize)
                .Include(c => c.ParentResub)
                .Include(c => c.ParentPost)
                .Include(c => c.ParentComment)
                .Include(c => c.Author)
                .Include(c => c.Votes)
                .Load();

            return post.Comments;
        }

        /// <summary>Create Comment In Post</summary>
        /// <remarks>Create a new comment in a post.</remarks>
        [HttpPost]
        [Route("{post_id}/comments")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [Authorize]
        public ActionResult<Comment> CreateCommentInPost([FromRoute(Name = "post_id")] int postId,
            CreateComment createComment)
        {
            var post = GetPost(postId);
            var comment = new Comment
            {
                Author = GetAuthorizedUser(),
                ParentPost = post,
                ParentResub = post.ParentResub,
                Content = createComment.Content,
                Created = DateTime.UtcNow
            };

            Db.Comments.Add(comment);
            Db.SaveChanges();
            Db.Entry(comment).Collection(c => c.Votes).Load();

            return CreatedAtRoute("GetComment", new {comment_id = comment.Id}, comment);
        }
    }
}