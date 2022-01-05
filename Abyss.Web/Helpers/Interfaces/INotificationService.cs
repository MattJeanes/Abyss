using Abyss.Web.Data;

namespace Abyss.Web.Helpers.Interfaces;

public interface INotificationHelper
{
    Task SendMessage(string message, MessagePriority messagePriority = MessagePriority.Normal);
}
