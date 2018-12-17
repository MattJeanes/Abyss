using Abyss.Web.Data.TeamSpeak;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Managers.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abyss.Web.Controllers
{
    [Route("api/online")]
    public class OnlineController : BaseController
    {
        private readonly IOnlineManager _manager;

        public OnlineController(IOnlineManager manager, IUserHelper userHelper) : base(userHelper)
        {
            _manager = manager;
        }

        [Route("client")]
        public async Task<List<Client>> GetClients()
        {
            return await _manager.GetClients();
        }

        [Route("channel")]
        public async Task<List<Channel>> GetChannels()
        {
            return await _manager.GetChannels();
        }
    }
}