using static dockertest.auth.JWTmanager;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using dockertest.models;


namespace dockertest.auth
{
    public interface IjwtManager
    {
        Token GetToken(User user);

    }
    public class JWTmanager : IjwtManager
    {
        private readonly IConfiguration config;
        public JWTmanager(IConfiguration config)
        {
            this.config = config;
        }
        public Token GetToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(config["JWT:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("UserID",user.Id.ToString(),ClaimValueTypes.Integer),

                }),
                Expires = DateTime.UtcNow.AddMinutes(10),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };
            var tokeData = tokenHandler.CreateToken(tokenDescriptor);
            var token = new Token { AccessToken = tokenHandler.WriteToken(tokeData) };
            return token;
        }
        public class Token
        {
            public string? AccessToken { get; set; }
        }
    }
}
