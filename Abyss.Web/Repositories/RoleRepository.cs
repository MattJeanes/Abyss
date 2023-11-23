using Abyss.Web.Contexts;
using Abyss.Web.Entities;
using Abyss.Web.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Abyss.Web.Repositories;

public class RoleRepository(AbyssContext context) : Repository<Role>(context), IRoleRepository
{
    public async override Task<Role> GetById(int id)
    {
        var role = await GetAll().Where(x => x.Id == id)
            .Include(x => x.Permissions)
            .FirstOrDefaultAsync();
        return role;
    }
}
