using Abyss.Web.Entities;
using Abyss.Web.Helpers.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Abyss.Web.Controllers;

[ApiController]
public class BaseController : Controller
{
    public readonly IUserHelper _userHelper;

    public BaseController(IUserHelper userHelper)
    {
        _userHelper = userHelper;
    }

    protected async Task<User> GetUser()
    {
        return await _userHelper.GetUser();
    }
}