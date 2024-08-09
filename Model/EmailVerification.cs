using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aspbackend.Model
    {
    [Table("EmailVerification")] // Adjust the table name to match your database
    public class EmailVerification
        {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? VerificationCode { get; set; }

        public DateTime ExpiryDate { get; set; }
        }
    }
