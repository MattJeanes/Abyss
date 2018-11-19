using Abyss.Web.Entities;
using Abyss.Web.Helpers.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss.Web.Controllers
{
    public class BaseController : Controller
    {
        public readonly IUserHelper _userHelper;

        public BaseController(IUserHelper userHelper)
        {
            _userHelper = userHelper;
        }

        public async Task<User> GetUser()
        {
            return await _userHelper.GetUser();
        }
    }
}
