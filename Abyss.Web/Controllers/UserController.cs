using Abyss.Web.Data;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Managers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace Abyss.Web.Controllers
{
    [Route("api/user")]
    [Authorize]
    public class UserController : BaseController
    {
        private readonly IUserManager _userManager;

        public UserController(IUserManager userManager, IUserHelper userHelper) : base(userHelper)
        {
            _userManager = userManager;
        }

        [Route("username")]
        [HttpPost]
        public async Task<AuthResult> ChangeUsername()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                var username = await reader.ReadToEndAsync();
                username = username.Trim();
                var user = await GetUser();
                await _userManager.ChangeUsername(user, username);
                var newToken = _userHelper.GetToken(user);
                return new AuthResult
                {
                    Token = newToken
                };
            }
        }
    }
}
