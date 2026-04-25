using System.Security.Cryptography;
using System.Text;
using Event.Application.IServices;

namespace Event.Application.Services
{
    public class PasswordGenerator : IPasswordGenerator
    {
        private const string Upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Lower = "abcdefghijklmnopqrstuvwxyz";
        private const string Digits = "0123456789";
        private const string Special = "!@#$%^&*";

        public string Generate(int length = 8)
        {
            var random = RandomNumberGenerator.Create();
            var result = new StringBuilder();

            result.Append(GetRandomChar(Upper, random));
            result.Append(GetRandomChar(Lower, random));
            result.Append(GetRandomChar(Digits, random));
            result.Append(GetRandomChar(Special, random));

            string all = Upper + Lower + Digits + Special;

            for (int i = result.Length; i < length; i++)
            {
                result.Append(GetRandomChar(all, random));
            }

            return Shuffle(result.ToString());
        }

        private char GetRandomChar(string chars, RandomNumberGenerator random)
        {
            var byteArray = new byte[1];
            random.GetBytes(byteArray);
            return chars[byteArray[0] % chars.Length];
        }

        private string Shuffle(string input)
        {
            var array = input.ToCharArray();
            var random = RandomNumberGenerator.Create();

            for (int i = array.Length - 1; i > 0; i--)
            {
                var byteArray = new byte[1];
                random.GetBytes(byteArray);
                int j = byteArray[0] % (i + 1);

                (array[i], array[j]) = (array[j], array[i]);
            }

            return new string(array);
        }
    }
}