using System.Security.Claims;

namespace Event.Application.Services
{
    internal class JwtSecurityToken
    {
        private object issuer;
        private object audience;
        private List<Claim> claims;
        private DateTime expires;
        private SigningCredentials signingCredentials;

        public JwtSecurityToken(object issuer, object audience, List<Claim> claims, DateTime expires, SigningCredentials signingCredentials)
        {
            this.issuer = issuer;
            this.audience = audience;
            this.claims = claims;
            this.expires = expires;
            this.signingCredentials = signingCredentials;
        }
    }
}