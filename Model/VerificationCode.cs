namespace Aspbackend.Model
{
    public class VerificationCode
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Email { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
