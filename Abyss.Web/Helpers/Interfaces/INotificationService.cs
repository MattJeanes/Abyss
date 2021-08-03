using Abyss.Web.Data;
using System.Threading.Tasks;

namespace Abyss.Web.Helpers.Interfaces
{
    public interface INotificationHelper
    {
        Task SendMessage(string message, MessagePriority messagePriority = MessagePriority.Normal);
    }
}
