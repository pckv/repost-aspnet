using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepostAspNet.Models;

namespace RepostAspNet.Controllers
{
    [Route("/api/comments")]
    public class CommentController : ApiControllerBase
    {
        public CommentController(RepostContext context) : base(context)
        {
        }

        /// <summary>Create Reply</summary>
        /// <remarks>Create a reply to a comment in a post.</remarks>
        [HttpPost]
        [Route("{comment_id}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [Authorize]
        public ActionResult<Comment> CreateReply([FromRoute(Name = "comment_id")] int commentId,
            CreateComment createComment)
        {
            var parentComment = GetComment(commentId);
            var comment = new Comment
            {
                Author = GetAuthorizedUser(),
                ParentComment = parentComment,
                ParentPost = parentComment.ParentPost,
                ParentResub = parentComment.ParentResub,
                Content = createComment.Content,
                Created = DateTime.UtcNow
            };

            Db.Comments.Add(comment);
            Db.SaveChanges();
            Db.Entry(comment).Collection(c => c.Votes).Load();

            return CreatedAtRoute("GetComment", new {comment_id = comment.Id}, comment);
        }

        /// <summary>Get Comment</summary>
        /// <remarks>Get a specific comment in a post.</remarks>
        [HttpGet]
        [Route("{comment_id}", Name = "GetComment")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public Comment GetComment([FromRoute(Name = "comment_id")] int commentId)
        {
            var comment = Db.Comments
                .Include(c => c.ParentResub)
                .Include(c => c.ParentPost)
                .Include(c => c.ParentComment)
                .Include(c => c.Author)
                .Include(c => c.Votes)
                .SingleOrDefault(c => c.Id == commentId);

            if (comment == null)
            {
                throw new StatusException(StatusCodes.Status404NotFound, $"Comment '{commentId}' not found");
            }

            return comment;
        }

        /// <summary>Edit Comment</summary>
        /// <remarks>
        ///     Edit a comment in a post.
        ///     Only the author of a post can edit the comment.
        /// </remarks>
        [HttpPatch]
        [Route("{comment_id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        [Authorize]
        public Comment EditComment([FromRoute(Name = "comment_id")] int commentId, EditComment editComment)
        {
            var user = GetAuthorizedUser();
            var comment = GetComment(commentId);
            if (!comment.IsAuthor(user))
            {
                throw new StatusException(StatusCodes.Status403Forbidden,
                    $"User '{user.Username}' lacks permission to edit comment '{comment.Id}'");
            }

            if (editComment.Content == null)
            {
                throw new StatusException(StatusCodes.Status422UnprocessableEntity,
                    "Failed to validate field content: can't be null");
            }

            comment.Content = editComment.Content;
            comment.Edited = DateTime.UtcNow;

            Db.SaveChanges();
            return comment;
        }

        /// <summary>Delete Comment</summary>
        /// <remarks>
        ///     Delete a comment in a post.
        ///     Only the author of a comment or the owner of the parent resub can delete the post.
        /// </remarks>
        [HttpDelete]
        [Route("{comment_id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [Authorize]
        public ActionResult DeleteComment([FromRoute(Name = "comment_id")] int commentId)
        {
            var user = GetAuthorizedUser();
            var comment = GetComment(commentId);
            if (!comment.CanDelete(user))
            {
                throw new StatusException(StatusCodes.Status403Forbidden,
                    $"User '{user.Username}' lacks permission to delete comment '{comment.Id}'");
            }

            Db.Remove(comment);
            Db.SaveChanges();
            return NoContent();
        }

        /// <summary>Vote Comment</summary>
        /// <remarks>Vote on a comment in a post.</remarks>
        [HttpPatch]
        [Route("{comment_id}/vote/{vote}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        [Authorize]
        public Comment VoteComment([FromRoute(Name = "comment_id")] int commentId, [Range(-1, 1)] int vote)
        {
            var author = GetAuthorizedUser();
            var comment = GetComment(commentId);

            var commentVote = Db.CommentsVotes.SingleOrDefault(c => c.User == author && c.Comment == comment);
            if (commentVote != null)
            {
                if (vote == 0)
                {
                    Db.CommentsVotes.Remove(commentVote);
                }
                else
                {
                    commentVote.Vote = vote;
                }
            }
            else
            {
                Db.CommentsVotes.Add(new CommentVote
                {
                    User = author,
                    Comment = comment,
                    Vote = vote
                });
            }

            Db.SaveChanges();
            Db.Entry(comment).Reload();
            return comment;
        }
    }
}