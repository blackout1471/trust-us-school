using IdentityApi.Interfaces;
using IdentityApi.Models;
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

        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult<UserToken>> Login(UserLogin userLogin)
        {
            try
            {

                var user = await _userManager.Login(userLogin);

                if (user == null)
                    return NotFound();

                return Ok(_tokenManager.GenerateUserToken(user));
            }
            catch (Exception e)
            {
                // log

                return Problem("E");
            }
        }
    }
}
