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
            _secret = _configuration["Jwt:Key"];
        }

        /// <inheritdoc/>
        public UserToken GetUserTokenFromToken(string token)
        {
            var principal = GetPrincipal(token);

            if (principal == null)
            {
                _logger.LogError($"Principal could not be extracted from token {token}");
                throw new MissingFieldException("Principal is missing from token");
            }

            ClaimsIdentity identity = (ClaimsIdentity)principal.Identity;

            return ClaimsIdentityToUserToken(identity, token);
        }

        /// <inheritdoc/>
        public UserToken GenerateUserToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Fills token with user information
            var claims = new[]
            {
                new Claim("ID", user.ID.ToString()),
                new Claim("EmailAdress", user.Email)
            };

            // Configuries the token
            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            UserToken userToken = new UserToken();
            userToken.Token = new JwtSecurityTokenHandler().WriteToken(token);
            userToken.UserID = user.ID;
            userToken.Email = user.Email;

            _logger.LogInformation($"User[{user.Email}] has created a new token");

            return userToken;
        }

        /// <inheritdoc/>
        public bool ValidateToken(string token)
        {
            ClaimsPrincipal principal = GetPrincipal(token);

            if (principal == null)
                return false;

            try
            {
                ClaimsIdentity identity = (ClaimsIdentity)principal.Identity;

                var userToken = ClaimsIdentityToUserToken(identity, token);
                _logger.LogInformation($"{token} has been validated");

                return userToken == null ? false : true;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"Token could not be validated: {token} in method ValidateToken");
                return false;
            }
        }

        /// <summary>
        /// Gets the token principal from the token
        /// </summary>
        private ClaimsPrincipal GetPrincipal(string token)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = (JwtSecurityToken)tokenHandler.ReadToken(token);

            if (jwtToken == null)
            {
                _logger.LogError($"Jwt Token is null {token} in method GetPrincipal");
                throw new InvalidDataException($"Jwt Token is null {token} in method GetPrincipal");
            }

            byte[] key = Encoding.UTF8.GetBytes(_secret);

            TokenValidationParameters parameters = new TokenValidationParameters()
            {
                RequireExpirationTime = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidAudience = _configuration["Jwt:Audience"],
                ValidIssuer = _configuration["Jwt:Issuer"],
            };

            return tokenHandler.ValidateToken(token, parameters, out SecurityToken securityToken);
        }

        /// <summary>
        /// Maps token fields to user token
        /// </summary>
        /// <returns>User token with values provided in the token</returns>
        private UserToken ClaimsIdentityToUserToken(ClaimsIdentity identity, string token)
        {
            var userToken = new UserToken();

            userToken.Email = identity.FindFirst("EmailAdress").Value;
            userToken.UserID = Convert.ToInt32(identity.FindFirst("id").Value);
            userToken.Token = token;

            return userToken;
        }
    }
}
