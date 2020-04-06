using System.Linq;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RepostAspNet.Models;

namespace RepostAspNet.Controllers
{
    [Route("/api/users")]
    public class UserController : ApiControllerBase
    {
        public UserController(DatabaseContext context) : base(context)
        {
        }

        /// <summary>Create User</summary>
        /// <remarks>Create a new user.</remarks>
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
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
                HashedPassword = BCrypt.Net.BCrypt.HashPassword(createUser.Password)
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
    }
}