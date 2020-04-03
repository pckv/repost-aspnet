using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RepostAspNet.Models;

namespace RepostAspNet.Controllers
{
    [Route("/api/users")]
    public class UserController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public UserController(DatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<IEnumerable<User>> GetUsers()
        {
            return _context.Users.ToList();
        }

        [HttpPost]
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