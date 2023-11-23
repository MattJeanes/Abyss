using Abyss.Web.Contexts;
using Abyss.Web.Data;
using Abyss.Web.Entities;
using Abyss.Web.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Abyss.Web.Repositories;

public class UserRepository(AbyssContext context) : Repository<User>(context), IUserRepository
{
    public async override Task<User> GetById(int id)
    {
        var user = await GetAll().Where(x => x.Id == id)
            .Include(x => x.Authentications)
            .Include(x => x.Role).ThenInclude(x => x.Permissions)
            .FirstOrDefaultAsync();
        return user;
    }

    public async Task<User> GetByExternalIdentifier(AuthSchemeType schemeType, string identifier)
    {
        var user = await _repository
            .Include(x => x.Authentications)
            .Include(x => x.Role).ThenInclude(x => x.Permissions)
            .FirstOrDefaultAsync(x => x.Authentications.Any(y => y.SchemeType == schemeType && y.Identifier == identifier));

        return user;
    }
}
