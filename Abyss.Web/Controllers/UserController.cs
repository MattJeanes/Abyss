using Abyss.Web.Data;
using Abyss.Web.Entities;
using Abyss.Web.Helpers;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Managers.Interfaces;
using Abyss.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Abyss.Web.Controllers
{
    [Route("api/user")]
    [Authorize]
    public class UserController : BaseController
    {
        private const string UserManagerPermission = "UserManager";
        private readonly IUserManager _userManager;
        private readonly IUserRepository _userRepository;

        public UserController(IUserManager userManager, IUserHelper userHelper, IUserRepository userRepository) : base(userHelper)
        {
            _userManager = userManager;
            _userRepository = userRepository;
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
                var newToken = await _userManager.ChangeUsername(user, username);
                return new AuthResult
                {
                    Token = newToken
                };
            }
        }

        [HttpGet]
        [AuthorizePermission(UserManagerPermission)]
        public async Task<List<User>> GetUsers()
        {
            return await _userRepository.GetAll().ToListAsync();
        }

        [HttpDelete]
        [Route("scheme/{schemeId}")]
        public async Task<AuthResult> DeleteAuthScheme(string schemeId)
        {
            var user = await GetUser();
            var token = await _userManager.DeleteAuthScheme(user, schemeId);

            return new AuthResult
            {
                Token = await _userHelper.GetAccessToken(user)
            };
        }
    }
}
