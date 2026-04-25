using Event.Application.IServices;
using System.Security.Cryptography;
using System.Text;

namespace Event.Application.Services
{
    public class PasswordGenerator : IPasswordGenerator
    {
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";

        public string Generate(int length = 12)
        {
            var res = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                res.Append(Chars[RandomNumberGenerator.GetInt32(Chars.Length)]);
            }
            return res.ToString();
        }
    }
}

