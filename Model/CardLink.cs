namespace Aspbackend.Model
{
    public class CardLink
    {
        public int Id { get; set; }
        public string? CardHolderName { get; set; }
        public string? CardNumber { get; set; }
        public string? CVV { get; set; }
        public string? ExpiryDate { get; set; } // MM/YY format
    }
}
