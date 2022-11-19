using IdentityApi.Interfaces;
using IdentityApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityApi.Managers
{
    public class TokenManager : ITokenManager
    {
        private readonly ILogger<TokenManager> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _secret;
        public TokenManager(IConfiguration configuration, ILogger<TokenManager> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _secret = _configuration["JWT:Key"];
        }

        public UserToken GetUserTokenFromToken(string token)
        {
            var principal = GetPrincipal(token);

            if (principal == null)
            {
                // TODO: log and change error message
                throw new Exception();
            }

            try
            {
                ClaimsIdentity identity = (ClaimsIdentity)principal.Identity;

                return ClaimsIdentityToUserToken(identity, token);
            }
            catch (Exception e)
            {
                // TODO: Log and change error

                throw e;
            }
        }

        public ClaimsPrincipal GetPrincipal(string token)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtToken = (JwtSecurityToken)tokenHandler.ReadToken(token);

                if (jwtToken == null)
                {
                    // TODO: Log and throw better error
                    throw new Exception();
                }

                byte[] key = Convert.FromBase64String(_secret);

                TokenValidationParameters parameters = new TokenValidationParameters()
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                return tokenHandler.ValidateToken(token, parameters, out SecurityToken securityToken);
            }
            catch (Exception e)
            {
                // TODO: Log and change message

                throw e;
            }
        }

        public UserToken GenerateUserToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("id", user.Id.ToString()),
                new Claim("email", user.Email)
            };

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            UserToken userToken = new UserToken();
            userToken.Token = new JwtSecurityTokenHandler().WriteToken(token);
            userToken.ID = user.Id;
            userToken.Email = user.Email;

            return userToken;
        }

        public bool ValidateToken(string token)
        {
            ClaimsPrincipal principal = GetPrincipal(token);

            if (principal == null)
                return false;

            try
            {
                ClaimsIdentity identity = (ClaimsIdentity)principal.Identity;

                var userToken = ClaimsIdentityToUserToken(identity, token);

                return userToken == null ? false : true;
            }
            catch (Exception e)
            {
                // TODO: Log

                return false;
            }
        }

        private UserToken ClaimsIdentityToUserToken(ClaimsIdentity identity, string token)
        {
            var userToken = new UserToken();

            userToken.Email = identity.FindFirst("email").Value;
            userToken.ID = Convert.ToInt32(identity.FindFirst("id").Value);
            userToken.Token = token;

            return userToken;
        }
    }
}
