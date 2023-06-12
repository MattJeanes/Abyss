using Abyss.Web.Data;
using Abyss.Web.Entities;

namespace Abyss.Web.Repositories.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User> GetByExternalIdentifier(AuthSchemeType schemeId, string identifier);
}
