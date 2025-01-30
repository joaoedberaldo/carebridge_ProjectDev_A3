using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CareBridgeBackend.Helpers
{
    public class JwtHelper
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _tokenExpirationInHours;

        public JwtHelper(IConfiguration configuration)
        {
            _secretKey = configuration["JwtSettings:SecretKey"];
            _issuer = configuration["JwtSettings:Issuer"];
            _audience = configuration["JwtSettings:Audience"];
            _tokenExpirationInHours = int.Parse(configuration["JwtSettings:TokenExpirationInHours"]);
        }

        public string GenerateToken(string userId, string role)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                _issuer,
                _audience,
                claims,
                expires: DateTime.UtcNow.AddHours(_tokenExpirationInHours),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
