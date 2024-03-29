﻿using Abyss.Web.Data;
using Abyss.Web.Entities;
using Abyss.Web.Helpers;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Managers.Interfaces;
using Abyss.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Abyss.Web.Controllers;

[Route("api/user")]
[Authorize]
public class UserController(IUserManager userManager, IUserHelper userHelper, IUserRepository userRepository) : BaseController(userHelper)
{
    private readonly IUserManager _userManager = userManager;
    private readonly IUserRepository _userRepository = userRepository;

    [Route("username")]
    [HttpPost]
    public async Task<AuthResult> ChangeUsername()
    {
        using (var reader = new StreamReader(Request.Body))
        {
            var username = await reader.ReadToEndAsync();
            username = username.Trim();
            var user = await GetUser();
            if (user == null) { throw new Exception("Invalid user"); }
            var newToken = await _userManager.ChangeUsername(user, username);
            return new AuthResult
            {
                Token = newToken
            };
        }
    }

    [HttpGet]
    [AuthorizePermission(Permissions.UserManager)]
    public async Task<List<User>> GetUsers()
    {
        return await _userRepository
            .GetAll()
            .Include(x => x.Authentications)
            .Include(x => x.Role)
            .ToListAsync();
    }

    [HttpDelete]
    [Route("scheme/{schemeType}")]
    public async Task<AuthResult> DeleteAuthScheme(AuthSchemeType schemeType)
    {
        var user = await GetUser();
        if (user == null) { throw new Exception("Invalid user"); }
        var token = await _userManager.DeleteAuthScheme(user, schemeType);

        return new AuthResult
        {
            Token = token
        };
    }
}
