using IdentityApi.Interfaces;
using IdentityApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApi.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserCreateController : ControllerBase
    {
        private readonly IUserManager _userManager;
        private readonly ITokenManager _tokenManager;
        public UserCreateController(IUserManager userManager, ITokenManager tokenManager)
        {
            _userManager = userManager;
            _tokenManager = tokenManager;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<ActionResult<UserToken>> Register(UserCreate userCreate)
        {
            try
            {
                var createdUser = await _userManager.CreateUser(userCreate);

                return Ok(_tokenManager.GenerateUserToken(createdUser));
            }
            catch (Exception e)
            {
                // TODO: Log

                return Problem("E");
            }
        }
    }
}
