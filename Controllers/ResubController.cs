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

        public ResubController(DatabaseContext context, UserController userController) : base(context)
        {
            _userController = userController;
        }

        /// <summary>Get Resubs</summary>
        /// <remarks>Get all resubs.</remarks>
        [HttpGet]
        public IEnumerable<Resub> GetResubs()
        {
            return Db.Resubs.ToList();
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
                Created = DateTime.Now
            };

            Db.Resubs.Add(resub);
            Db.SaveChanges();

            return CreatedAtAction(nameof(GetResub), new {name = resub.Name}, resub);
        }

        /// <summary>Get Resub</summary>
        /// <remarks>Get a specific resub.</remarks>
        [HttpGet]
        [Route("{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public Resub GetResub(string name)
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
        [Route("{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [Authorize]
        public Resub EditResub(string name, EditResub editResub)
        {
            var resub = GetResub(name);
            if (!resub.IsAllowedToEdit(GetAuthorizedUser()))
            {
                throw new StatusException(StatusCodes.Status403Forbidden, "You are not the owner of the resub");
            }

            if (editResub.IsFieldSet(nameof(editResub.Description)))
                resub.Description = editResub.Description;
            if (editResub.NewOwnerUsername != null)
            {
                var newOwner = _userController.GetUser(editResub.NewOwnerUsername);
                resub.Owner = newOwner;
            }

            Db.SaveChanges();
            return resub;
        }

        /// <summary>Delete Resub</summary>
        /// <remarks>
        ///     Delete a resub.
        ///     Only the owner of a resub can delete the resub.
        /// </remarks>
        [HttpDelete]
        [Route("{name}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [Authorize]
        public ActionResult DeleteResub(string name)
        {
            var resub = GetResub(name);
            if (!resub.IsAllowedToEdit(GetAuthorizedUser()))
            {
                throw new StatusException(StatusCodes.Status403Forbidden, "You are not the owner of the resub");
            }

            Db.Remove(resub);
            Db.SaveChanges();
            return NoContent();
        }
    }
}