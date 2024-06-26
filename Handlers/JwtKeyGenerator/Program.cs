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

// Y8u9Gc7PMRlFBXy3+1wiTRNb97T5uI0VDxP5hLTzwPw=