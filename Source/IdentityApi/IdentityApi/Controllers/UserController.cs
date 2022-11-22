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
        UserLocation userLocation = null;
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
        [Route("Create")]
        public async Task<ActionResult<UserToken>> Create(UserCreate userCreate)
        {
            try
            {
                var createdUser = await _userManager.CreateUserAsync(userCreate);

                return Ok(_tokenManager.GenerateUserToken(createdUser));
            }
            catch (Exception e)
            {
                // TODO: Log

                return Problem("E");
            }
        }

        /// <summary>
        /// Logs user in, if exists
        /// </summary>
        /// <returns>User Token</returns>
        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult<UserToken>> Login(UserLogin userLogin)
        {
            try
            {
                var user = await _userManager.LoginAsync(userLogin);

                if (user == null)
                    return NoContent();

                return Ok(_tokenManager.GenerateUserToken(user));
            }
            catch (Exception e)
            {
                // TODO: log

                return Problem("E");
            }
        }

        /// <summary>
        /// Logs user in, if exists
        /// </summary>
        /// <returns>User Token</returns>
        [Authorize]
        [HttpGet]
        [Route("GetUserByToken")]
        public async Task<ActionResult<User>> GetUserByToken()
        {
            try
            {
                var tokenUser = _tokenManager.GetUserTokenFromToken(Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Replace("bearer ", ""));

                if (tokenUser == null)
                    return Problem("E");

                var user = await _userManager.GetUserByIDAsync(tokenUser.UserID);

                if (user == null)
                    return NoContent();

                return Ok(user);
            }
            catch (Exception e)
            {
                // TODO: log

                return Problem("E");
            }
        }
    }
}
