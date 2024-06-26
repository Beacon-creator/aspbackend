using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Aspbackend.Model;


[Table("User")] // Adjust the table name to match your database
public class User
{
    public int Id { get; set; }

 
    public string? FullName { get; set; }

    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    public string? Phonenumber { get; set; }

    public byte[] PasswordHash { get; set; }

    public byte[] PasswordSalt { get; set; }

    public bool Terms { get; set; }

    [NotMapped]
    public string? Password { get; set; }  // Temporary property for processing
}