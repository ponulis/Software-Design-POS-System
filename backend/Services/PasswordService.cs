using System.Security.Cryptography;
using System.Text;

namespace backend.Services
{
    public class PasswordService
    {
        // Just for now, maybe we use something more robust later
        public string HashPassword(string password)
        {
            // Simple SHA256 hash (not ideal for production, but ok for demo)
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == passwordHash;
        }
    }
}