using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Aspbackend.Data;
using Aspbackend.Model;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;


namespace Aspbackend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CardLinksController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CardLinksController> _logger;

        public CardLinksController(AppDbContext context, ILogger<CardLinksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/CardLinks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CardLink>>> GetCardLinks()
        {
            try
            {
                return await _context.CardLinks.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching card links.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        // GET: api/CardLinks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CardLink>> GetCardLink(int id)
        {
            try
            {
                var cardLink = await _context.CardLinks.FindAsync(id);

                if (cardLink == null)
                {
                    return NotFound();
                }

                return cardLink;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching the card link.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        // PUT: api/CardLinks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCardLink(int id, CardLink cardLink)
        {
            if (id != cardLink.Id)
            {
                return BadRequest();
            }

            _context.Entry(cardLink).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CardLinkExists(id))
                {
                    return NotFound();
                }
                else
                {
                    _logger.LogError("Concurrency error occurred while updating the card link.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating the card link.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }

            return NoContent();
        }

        // POST: api/CardLinks
        [HttpPost]
        public async Task<ActionResult<CardLink>> PostCardLink(CardLink cardLink)
        {
            try
            {
                _context.CardLinks.Add(cardLink);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetCardLink", new { id = cardLink.Id }, cardLink);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating the card link.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        // DELETE: api/CardLinks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCardLink(int id)
        {
            try
            {
                var cardLink = await _context.CardLinks.FindAsync(id);
                if (cardLink == null)
                {
                    return NotFound();
                }

                _context.CardLinks.Remove(cardLink);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting the card link.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        private bool CardLinkExists(int id)
        {
            return _context.CardLinks.Any(e => e.Id == id);
        }
    }
}
