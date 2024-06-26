namespace Aspbackend.Model
{
    public class PasswordReset
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? VerificationCode { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
