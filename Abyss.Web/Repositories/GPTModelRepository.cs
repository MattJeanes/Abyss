using Abyss.Web.Contexts;
using Abyss.Web.Entities;
using Abyss.Web.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Abyss.Web.Repositories;

public class GPTModelRepository(AbyssContext context) : Repository<GPTModel>(context), IGPTModelRepository
{
    public async override Task<GPTModel> GetById(int id)
    {
        var role = await GetAll().Where(x => x.Id == id)
            .Include(x => x.Permission)
            .FirstOrDefaultAsync();
        return role;
    }
}
