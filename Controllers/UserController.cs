using System.Linq;
using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RepostAspNet.Models;

namespace RepostAspNet.Controllers
{
    [Route("/api/users")]
    [Produces(MediaTypeNames.Application.Json)]
    public class UserController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public UserController(DatabaseContext context)
        {
            _context = context;
        }

        /// <summary>Create user</summary>
        /// <remarks>Create a new user.</remarks>
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public ActionResult<User> CreateUser(CreateUser createUser)
        {
            if (_context.Users.Any(u => u.Username == createUser.Username))
            {
                return BadRequest($"User '{createUser.Username}' already exists");
            }

            var user = new User
            {
                Username = createUser.Username,
                HashedPassword = BCrypt.Net.BCrypt.HashPassword(createUser.Password)
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetUser), new {username = user.Username}, user);
        }

        /// <summary>Get user</summary>
        /// <remarks>Get a specific user.</remarks>
        [HttpGet]
        [Route("{username}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public ActionResult<User> GetUser(string username)
        {
            var user = _context.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
            {
                return NotFound($"User '{username}' not found");
            }

            return user;
        }
    }
}