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

namespace Aspbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankLinksController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BankLinksController> _logger;
        private readonly EmailService _emailService;

        public BankLinksController(AppDbContext context, ILogger<BankLinksController> logger, EmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        // GET: api/BankLinks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BankLink>>> GetBankLinks()
        {
            try
            {
                return await _context.BankLinks.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching bank links.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        // GET: api/BankLinks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BankLink>> GetBankLink(int id)
        {
            try
            {
                var bankLink = await _context.BankLinks.FindAsync(id);

                if (bankLink == null)
                {
                    return NotFound();
                }

                return bankLink;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching the bank link.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        // PUT: api/BankLinks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBankLink(int id, BankLink bankLink)
        {
            if (id != bankLink.Id)
            {
                return BadRequest();
            }

            _context.Entry(bankLink).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BankLinkExists(id))
                {
                    return NotFound();
                }
                else
                {
                    _logger.LogError("Concurrency error occurred while updating the bank link.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating the bank link.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }

            return NoContent();
        }

        // POST: api/BankLinks
        [HttpPost]
        public async Task<ActionResult<BankLink>> PostBankLink(BankLink bankLink)
        {
            try
            {
                _context.BankLinks.Add(bankLink);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetBankLink", new { id = bankLink.Id }, bankLink);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating the bank link.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        // DELETE: api/BankLinks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBankLink(int id)
        {
            try
            {
                var bankLink = await _context.BankLinks.FindAsync(id);
                if (bankLink == null)
                {
                    return NotFound();
                }

                _context.BankLinks.Remove(bankLink);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting the bank link.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        private bool BankLinkExists(int id)
        {
            return _context.BankLinks.Any(e => e.Id == id);
        }

        // POST: api/BankLinks/SendVerificationCode
        [HttpPost("SendVerificationCode")]
        public async Task<IActionResult> SendVerificationCode([FromBody] string email)
        {
            try
            {
                var verificationCode = new VerificationCode
                {
                    Code = GenerateVerificationCode(),
                    Email = email,
                    ExpiryDate = DateTime.UtcNow.AddMinutes(15)
                };

                _context.VerificationCodes.Add(verificationCode);
                await _context.SaveChangesAsync();

                // Send email using EmailService
                await _emailService.SendEmailAsync(email, "Your Verification Code", $"Your verification code is {verificationCode.Code}");

                return Ok("Verification code sent successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending verification code.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        // POST: api/BankLinks/VerifyCode
        [HttpPost("VerifyCode")]
        public async Task<IActionResult> VerifyCode([FromBody] VerificationRequest request)
        {
            try
            {
                var verificationCode = await _context.VerificationCodes
                    .FirstOrDefaultAsync(vc => vc.Email == request.Email && vc.Code == request.Code);

                if (verificationCode == null || verificationCode.ExpiryDate < DateTime.UtcNow)
                {
                    return BadRequest("Invalid or expired verification code.");
                }

                return Ok("Verification successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while verifying code.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        private string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(1000, 9999).ToString();
        }

    }
}
