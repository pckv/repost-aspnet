using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepostAspNet.Models;

namespace RepostAspNet.Controllers
{
    [Route("/api/resubs")]
    public class ResubController : ApiControllerBase
    {
        private readonly UserController _userController;

        public ResubController(RepostContext context, UserController userController) : base(context)
        {
            _userController = userController;
        }

        /// <summary>Get Resubs</summary>
        /// <remarks>Get all resubs.</remarks>
        [HttpGet]
        public IEnumerable<Resub> GetResubs(int page = 0, [FromQuery(Name = "page_size")] int pageSize = 100)
        {
            return Db.Resubs
                .Skip(page * pageSize)
                .Take(pageSize)
                .Include(r => r.Owner)
                .ToList();
        }

        /// <summary>Create Resub</summary>
        /// <remarks>Create a new resub.</remarks>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [Authorize]
        public ActionResult<Resub> CreateResub(CreateResub createResub)
        {
            var owner = GetAuthorizedUser();

            if (Db.Resubs.Any(r => r.Name == createResub.Name))
            {
                throw new StatusException(StatusCodes.Status400BadRequest,
                    $"Resub '{createResub.Name}' already exists");
            }

            var resub = new Resub
            {
                Owner = owner,
                Name = createResub.Name,
                Description = createResub.Description,
                Created = DateTime.UtcNow
            };

            Db.Resubs.Add(resub);
            Db.SaveChanges();

            return CreatedAtAction(nameof(GetResub), new {resub = resub.Name}, resub);
        }

        /// <summary>Get Resub</summary>
        /// <remarks>Get a specific resub.</remarks>
        [HttpGet]
        [Route("{resub}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public Resub GetResub([FromRoute(Name = "resub")] string name)
        {
            var resub = Db.Resubs.Include(r => r.Owner).SingleOrDefault(r => r.Name == name);
            if (resub == null)
            {
                throw new StatusException(StatusCodes.Status404NotFound, $"Resub '{name}' not found");
            }

            return resub;
        }

        /// <summary>Edit Resub</summary>
        /// <remarks>
        ///     Edit a resub.
        ///     Only the owner of a resub can edit the resub.
        /// </remarks>
        [HttpPatch]
        [Route("{resub}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [Authorize]
        public Resub EditResub([FromRoute(Name = "resub")] string name, EditResub editResub)
        {
            var user = GetAuthorizedUser();
            var resub = GetResub(name);
            if (!resub.IsOwner(user))
            {
                throw new StatusException(StatusCodes.Status403Forbidden,
                    $"User '{user.Username}' lacks permission to edit resub '{resub.Name}'");
            }

            if (editResub.IsFieldSet(nameof(editResub.Description)))
                resub.Description = editResub.Description;
            if (editResub.NewOwnerUsername != null)
                resub.Owner = _userController.GetUser(editResub.NewOwnerUsername);
            resub.Edited = DateTime.UtcNow;

            Db.SaveChanges();
            return resub;
        }

        /// <summary>Delete Resub</summary>
        /// <remarks>
        ///     Delete a resub.
        ///     Only the owner of a resub can delete the resub.
        /// </remarks>
        [HttpDelete]
        [Route("{resub}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [Authorize]
        public ActionResult DeleteResub([FromRoute(Name = "resub")] string name)
        {
            var user = GetAuthorizedUser();
            var resub = GetResub(name);
            if (!resub.CanDelete(user))
            {
                throw new StatusException(StatusCodes.Status403Forbidden,
                    $"User '{user.Username}' lacks permission to delete resub '{resub.Name}'");
            }

            Db.Remove(resub);
            Db.SaveChanges();
            return NoContent();
        }

        /// <summary>Get Posts In Resub</summary>
        /// <remarks>Get all posts in a resub.</remarks>
        [HttpGet]
        [Route("{resub}/posts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public IEnumerable<Post> GetPostsInResub([FromRoute(Name = "resub")] string name, int page = 0,
            [FromQuery(Name = "page_size")] int pageSize = 100)
        {
            var resub = GetResub(name);
            Db.Entry(resub)
                .Collection(r => r.Posts).Query()
                .Skip(page * pageSize)
                .Take(pageSize)
                .Include(p => p.Author)
                .Include(p => p.Votes)
                .Load();

            return resub.Posts;
        }

        /// <summary>Create Post In Resub</summary>
        /// <remarks>Create a new post in a resub.</remarks>
        [HttpPost]
        [Route("{resub}/posts")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [Authorize]
        public ActionResult<Post> CreatePostInResub([FromRoute(Name = "resub")] string name, CreatePost createPost)
        {
            var post = new Post
            {
                Author = GetAuthorizedUser(),
                ParentResub = GetResub(name),
                Title = createPost.Title,
                Content = createPost.Content,
                Url = createPost.Url,
                Created = DateTime.UtcNow
            };

            Db.Posts.Add(post);
            Db.SaveChanges();
            Db.Entry(post).Collection(p => p.Votes).Load();

            return CreatedAtRoute("GetPost", new {post_id = post.Id}, post);
        }
    }
}