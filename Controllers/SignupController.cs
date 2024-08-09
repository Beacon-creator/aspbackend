using Aspbackend.Data;
using Aspbackend.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Aspbackend.Controllers
    {
    [Route("api/[controller]")]
    [ApiController]
    public class SignupController : ControllerBase
        {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private readonly ILogger<SignupController> _logger;

        public SignupController(AppDbContext context, IConfiguration configuration, EmailService emailService, ILogger<SignupController> logger)
            {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _logger = logger;
            }

        // GET: api/Users/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<User>> GetUser(int id)
            {
            try
                {
                var user = await _context.Users.FindAsync(id);

                if (user == null)
                    {
                    return NotFound();
                    }

                return user;
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error occurred while fetching the user.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
                }
            }

        [HttpPost("")]
        public async Task<ActionResult<User>> Signup([FromBody] SignupModel signupModel)
            {
            if (ModelState.IsValid)
                {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(x => x.Email.ToLower() == signupModel.Email.ToLower());

                if (existingUser != null)
                    {
                    return Conflict("User with this email already exists");
                    }

                CreatePasswordHash(signupModel.Password, out byte[] passwordHash, out byte[] passwordSalt);

                var user = new User
                    {
                    Email = signupModel.Email,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    IsEmailVerified = false // Assuming you have a field to track email verification status
                    };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Generate and send verification code
                var verificationCode = GenerateVerificationCode();
                var expiryDate = DateTime.UtcNow.AddHours(1);

                var emailVerification = new EmailVerification
                    {
                    Email = signupModel.Email,
                    VerificationCode = verificationCode,
                    ExpiryDate = expiryDate
                    };

                _context.EmailVerifications.Add(emailVerification);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Verification code {verificationCode} sent to {signupModel.Email} with expiry {expiryDate}.");

                var subject = "Email Verification Code";
                var message = $"Your email verification code is {verificationCode}.";
                await _emailService.SendEmailAsync(signupModel.Email, subject, message);

                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
                }

            return BadRequest("Invalid request");
            }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerificationModel verificationModel)
            {
            _logger.LogInformation($"Verifying email with code {verificationModel.Code} for email {verificationModel.Email}.");

            var emailVerification = await _context.EmailVerifications
                .FirstOrDefaultAsync(ev => ev.Email.ToLower() == verificationModel.Email.ToLower() &&
                                           ev.VerificationCode == verificationModel.Code &&
                                           ev.ExpiryDate > DateTime.UtcNow);

            if (emailVerification == null)
                {
                _logger.LogWarning($"Invalid or expired email verification code for email {verificationModel.Email}.");
                return BadRequest("Invalid or expired verification code.");
                }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == verificationModel.Email.ToLower());
            if (user == null)
                {
                return NotFound("User with this email does not exist.");
                }

            user.IsEmailVerified = true;
            _context.Users.Update(user);
            _context.EmailVerifications.Remove(emailVerification); // Remove used verification code
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Email verified successfully for {verificationModel.Email}.");

            return Ok("Email verified successfully.");
            }

        private string GenerateVerificationCode()
            {
            return new Random().Next(100000, 999999).ToString();
            }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
            {
            using (var hmac = new HMACSHA512())
                {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                }
            }
        }
    }
