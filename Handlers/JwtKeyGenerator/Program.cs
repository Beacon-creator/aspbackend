using System;
using System.Security.Cryptography;

 partial class Program
{
    static void Main()
    {
        var key = GenerateSecureKey();
        Console.WriteLine($"Generated JWT Key: {key}");
    }

    static string GenerateSecureKey()
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            var keyBytes = new byte[32]; // 256-bit key
            rng.GetBytes(keyBytes);
            return Convert.ToBase64String(keyBytes);
        }
    }
}
