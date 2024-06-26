using Aspbackend.Data;
using Aspbackend.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging; // Add logging
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Aspbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordResetController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private readonly ILogger<PasswordResetController> _logger; // Add logger

        public PasswordResetController(AppDbContext context, IConfiguration configuration, EmailService emailService, ILogger<PasswordResetController> logger)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _logger = logger; // Initialize logger
        }

        [HttpPost("send-code")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            if (user == null)
            {
                return NotFound("User with this email does not exist.");
            }

            var verificationCode = GenerateVerificationCode();
            var expiryDate = DateTime.UtcNow.AddHours(1);

            var passwordReset = new PasswordReset
            {
                Email = email,
                VerificationCode = verificationCode,
                ExpiryDate = expiryDate
            };

            _context.PasswordResets.Add(passwordReset);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Verification code {verificationCode} sent to {email} with expiry {expiryDate}.");

            var subject = "Password Reset Verification Code";
            var message = $"Your verification code is {verificationCode}";
            await _emailService.SendEmailAsync(email, subject, message);

            return Ok(verificationCode);
        }

        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode([FromBody] VerificationModel verificationModel)
        {
            _logger.LogInformation($"Verifying code {verificationModel.Code} for email {verificationModel.Email}.");

            var passwordReset = await _context.PasswordResets
                .FirstOrDefaultAsync(pr => pr.Email.ToLower() == verificationModel.Email.ToLower() &&
                                            pr.VerificationCode == verificationModel.Code &&
                                            pr.ExpiryDate > DateTime.UtcNow);

            if (passwordReset == null)
            {
                _logger.LogWarning($"Invalid or expired verification code for email {verificationModel.Email}.");
                return BadRequest("Invalid or expired verification code.");
            }

            _logger.LogInformation($"Verification code {verificationModel.Code} for email {verificationModel.Email} is valid.");

            return Ok("Verification code is valid.");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel resetPasswordModel)
        {
            _logger.LogInformation($"Resetting password for email {resetPasswordModel.Email} with code {resetPasswordModel.Code}.");

            var passwordReset = await _context.PasswordResets
                .FirstOrDefaultAsync(pr => pr.Email.ToLower() == resetPasswordModel.Email.ToLower() &&
                                            pr.VerificationCode == resetPasswordModel.Code &&
                                            pr.ExpiryDate > DateTime.UtcNow);

            if (passwordReset == null)
            {
                _logger.LogWarning($"Invalid or expired verification code for email {resetPasswordModel.Email}.");
                return BadRequest("Invalid or expired verification code.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == resetPasswordModel.Email.ToLower());
            if (user == null)
            {
                return NotFound("User with this email does not exist.");
            }

            CreatePasswordHash(resetPasswordModel.Password, out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _context.Users.Update(user);
            _context.PasswordResets.Remove(passwordReset);  // Remove the used token
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Password has been reset successfully for email {resetPasswordModel.Email}.");

            return Ok("Password has been reset successfully.");
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
