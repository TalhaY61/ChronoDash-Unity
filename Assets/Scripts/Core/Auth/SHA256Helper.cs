using System;
using System.Security.Cryptography;
using System.Text;

namespace ChronoDash.Core.Auth
{
    /// <summary>
    /// SHA-256 password hashing utility.
    /// CRITICAL: All passwords must be hashed before sending to backend.
    /// </summary>
    public static class SHA256Helper
    {
        /// <summary>
        /// Hash a password using SHA-256 algorithm.
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <returns>SHA-256 hashed password as hex string</returns>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be null or empty", nameof(password));
            }
            
            using (SHA256 sha256 = SHA256.Create())
            {
                // Convert password to bytes
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                
                // Compute hash
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                
                // Convert to hex string
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                
                return sb.ToString();
            }
        }
        
        /// <summary>
        /// Verify if a plain text password matches a hash.
        /// </summary>
        public static bool VerifyPassword(string password, string hash)
        {
            string passwordHash = HashPassword(password);
            return string.Equals(passwordHash, hash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
