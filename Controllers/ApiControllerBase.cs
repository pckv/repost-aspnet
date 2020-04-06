using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RepostAspNet.Models;

namespace RepostAspNet.Controllers
{
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    public class ApiControllerBase : ControllerBase
    {
        protected readonly DatabaseContext Db;

        public ApiControllerBase(DatabaseContext context)
        {
            Db = context;
        }

        protected User GetAuthorizedUser()
        {
            var username = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = Db.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
            {
                throw new StatusException(StatusCodes.Status401Unauthorized, $"User '{username}' not found");
            }

            return user;
        }
    }
}