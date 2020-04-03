using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RepostAspNet.Models;

namespace RepostAspNet.Controllers
{
    [Route("/api/resubs")]
    public class ResubController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public ResubController(DatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Resub>> GetResubs()
        {
            return _context.Resubs.ToList();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public ActionResult<Resub> CreateResub(CreateResub createResub)
        {
            if (_context.Resubs.Any(r => r.Name == createResub.Name))
            {
                return BadRequest($"Resub '{createResub.Name}' already exists");
            }
            
            var resub = new Resub
            {
                Name = createResub.Name,
                Description = createResub.Description
            };
            
            _context.Resubs.Add(resub);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetResub), new {name = resub.Name}, resub);
        }

        [HttpGet]
        [Route("{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public ActionResult<Resub> GetResub(string name)
        {
            var resub = _context.Resubs.SingleOrDefault(r => r.Name == name);
            if (resub == null)
            {
                return NotFound($"Resub '{name}' not found");
            }

            return resub;
        }
    }
}