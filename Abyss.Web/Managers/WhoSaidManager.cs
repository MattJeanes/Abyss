using Abyss.Web.Data;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Managers.Interfaces;
using System.Threading.Tasks;

namespace Abyss.Web.Managers
{
    public class WhoSaidManager : IWhoSaidManager
    {
        private readonly IWhoSaidHelper _whoSaidHelper;

        public WhoSaidManager(IWhoSaidHelper whoSaidHelper) {
            _whoSaidHelper = whoSaidHelper;
        }
        
        public async Task<WhoSaid> WhoSaid(string message)
        {
            return await _whoSaidHelper.WhoSaid(message);
        }
    }
}
