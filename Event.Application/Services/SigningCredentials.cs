namespace Event.Application.Services
{
    internal class SigningCredentials
    {
        private SymmetricSecurityKey key;
        private object hmacSha256;

        public SigningCredentials(SymmetricSecurityKey key, object hmacSha256)
        {
            this.key = key;
            this.hmacSha256 = hmacSha256;
        }
    }
}