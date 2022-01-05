using Abyss.Web.Contexts.Interfaces;
using Abyss.Web.Entities;
using Abyss.Web.Repositories.Interfaces;
using MongoDB.Driver;

namespace Abyss.Web.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(IAbyssContext context) : base(context) { }

    public async Task<User> GetByExternalIdentifier(string schemeId, string identifier)
    {
        var builders = Builders<User>.Filter;
        var filter = builders.Eq($"{nameof(User.Authentication)}.{schemeId}", identifier);
        var user = await _repository.Find(filter).FirstOrDefaultAsync();

        return user;
    }
}
