using IdentityApi.Exceptions;
using IdentityApi.Interfaces;
using IdentityApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApi.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserManager _userManager;
        private readonly ITokenManager _tokenManager;

        public UserController(IUserManager userManager, ITokenManager tokenManager)
        {
            _userManager = userManager;
            _tokenManager = tokenManager;
        }

        ///<summary>
        /// Registers user if it doesn't exist
        /// </summary>
        /// <returns>User token for newly created user</returns>
        [HttpPost]
        [Route("create")]
        public async Task<ActionResult> CreateAsync(UserCreate userCreate)
        {
            return await _userManager.CreateUserAsync(userCreate, GetUserLocation()) ? 
                Ok("User has been created!") : throw new Exception("Something went wrong! try again");
        }

        /// <summary>
        /// Logs user in, if exists
        /// </summary>
        /// <returns>User Token</returns>
        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<UserToken>> LoginAsync(UserLogin userLogin)
        {
            var user = await _userManager.LoginAsync(userLogin, GetUserLocation());

            if (user == null)
                throw new UserIncorrectLoginException();

            return Ok(_tokenManager.GenerateUserToken(user));
        }

        /// <summary>
        /// Logs user in with onetime password, if exists
        /// </summary>
        /// <returns>User Token</returns>
        [HttpPost]
        [Route("verificationlogin")]
        public async Task<ActionResult<UserToken>> VerificationLoginAsync(VerifyCredentials verifyCredentials)
        {
            var user = await _userManager.LoginWithVerificationCodeAsync(verifyCredentials, GetUserLocation());

            if (user == null)
                throw new UserIncorrectLoginException();

            return Ok(_tokenManager.GenerateUserToken(user));
        }

        /// <summary>
        /// Logs user in with onetime password, if exists
        /// </summary>
        /// <returns>Ok if user is verified</returns>
        [HttpPost]
        [Route("verifyregister")]
        public async Task<ActionResult> VerifyUserRegisterAsync(VerifyCredentials verifyCredentials)
        {
            return await _userManager.VerifyUserRegistrationAsync(verifyCredentials, GetUserLocation()) ? 
                Ok() : throw new UserIncorrectLoginException();
        }

        /// <summary>
        /// Logs user in, if exists
        /// </summary>
        /// <returns>User Token</returns>
        [Authorize]
        [HttpGet]
        [Route("getuserbytoken")]
        public async Task<ActionResult<User>> GetUserByTokenAsync()
        {
            var tokenUser = _tokenManager.GetUserTokenFromToken(Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Replace("bearer ", ""));

            if (tokenUser == null)
                return NoContent();

            var user = await _userManager.GetUserByIDAsync(tokenUser.UserID);

            if (user == null)
                return NoContent();

            return Ok(user);
        }

        /// <summary>
        /// Retrieves the user location data from the request context
        /// </summary>
        private UserLocation GetUserLocation()
        {
            var userLocation = new UserLocation();
            userLocation.IP = Request?.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";

            userLocation.UserAgent = Request?.Headers["User-Agent"].ToString() ?? "Unknown";

            return userLocation;
        }
    }
}

