using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepostAspNet.Models;

namespace RepostAspNet.Controllers
{
    [Route("/api/users")]
    public class UserController : ApiControllerBase
    {
        public UserController(RepostContext context) : base(context)
        {
        }

        /// <summary>Create User</summary>
        /// <remarks>Create a new user.</remarks>
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public ActionResult<User> CreateUser(CreateUser createUser)
        {
            if (Db.Users.Any(u => u.Username == createUser.Username))
            {
                throw new StatusException(StatusCodes.Status400BadRequest,
                    $"User '{createUser.Username}' already exists");
            }

            var user = new User
            {
                Username = createUser.Username,
                HashedPassword = BCrypt.Net.BCrypt.HashPassword(createUser.Password),
                Created = DateTime.UtcNow
            };

            Db.Users.Add(user);
            Db.SaveChanges();
            return CreatedAtAction(nameof(GetUser), new {username = user.Username}, user);
        }

        /// <summary>Get Current User</summary>
        /// <remarks>Get the currently authorized user.</remarks>
        [HttpGet]
        [Route("me")]
        [Authorize]
        public User GetCurrentUser()
        {
            return GetAuthorizedUser();
        }

        /// <summary>Edit Current User</summary>
        /// <remarks>Edit the currently authorized user.</remarks>
        [HttpPatch]
        [Route("me")]
        [Authorize]
        public User EditCurrentUser(EditUser editUser)
        {
            var user = GetAuthorizedUser();

            if (editUser.IsFieldSet(nameof(editUser.Bio)))
                user.Bio = editUser.Bio;
            if (editUser.IsFieldSet(nameof(editUser.AvatarUrl)))
                user.AvatarUrl = editUser.AvatarUrl;
            user.Edited = DateTime.UtcNow;

            Db.SaveChanges();
            return user;
        }

        /// <summary>Delete Current User</summary>
        /// <remarks>Delete the currently authorized user.</remarks>
        [HttpDelete]
        [Route("me")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize]
        public ActionResult DeleteCurrentUser()
        {
            var user = GetAuthorizedUser();

            Db.Users.Remove(user);
            Db.SaveChanges();
            return NoContent();
        }

        /// <summary>Get User</summary>
        /// <remarks>Get a specific user.</remarks>
        [HttpGet]
        [Route("{username}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public User GetUser(string username)
        {
            var user = Db.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
            {
                throw new StatusException(StatusCodes.Status404NotFound, $"User '{username}' not found");
            }

            return user;
        }

        /// <summary>Get Resubs Owned By User</summary>
        /// <remarks>Get all resubs owned by a specific user.</remarks>
        [HttpGet]
        [Route("{username}/resubs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public IEnumerable<Resub> GetResubsOwnedByUser(string username, int page = 0,
            [FromQuery(Name = "page_size")] int pageSize = 100)
        {
            var user = GetUser(username);
            Db.Entry(user)
                .Collection(u => u.Resubs).Query()
                .OrderByDescending(r => r.Created)
                .Skip(page * pageSize)
                .Take(pageSize)
                .Load();
            return user.Resubs;
        }

        /// <summary>Get Posts By User</summary>
        /// <remarks>Get all posts by a specific user.</remarks>
        [HttpGet]
        [Route("{username}/posts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public IEnumerable<Post> GetPostsByUser(string username, int page = 0,
            [FromQuery(Name = "page_size")] int pageSize = 100)
        {
            var user = GetUser(username);
            Db.Entry(user)
                .Collection(u => u.Posts).Query()
                .OrderByDescending(p => p.Created)
                .Skip(page * pageSize)
                .Take(pageSize)
                .Include(p => p.ParentResub)
                .Include(p => p.Votes)
                .Load();

            return user.Posts;
        }

        /// <summary>Get Comments By User</summary>
        /// <remarks>Get all comments by a specific user.</remarks>
        [HttpGet]
        [Route("{username}/comments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public IEnumerable<Comment> GetCommentsByUser(string username, int page = 0,
            [FromQuery(Name = "page_size")] int pageSize = 100)
        {
            var user = GetUser(username);
            Db.Entry(user)
                .Collection(u => u.Comments).Query()
                .OrderByDescending(c => c.Created)
                .Skip(page * pageSize)
                .Take(pageSize)
                .Include(c => c.ParentResub)
                .Include(c => c.ParentPost)
                .Include(c => c.ParentComment)
                .Include(c => c.Votes)
                .Load();

            return user.Comments;
        }
    }
}