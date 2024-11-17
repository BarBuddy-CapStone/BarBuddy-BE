using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IFcmService
    {
        Task SaveUserDeviceToken(Guid accountId, string deviceToken, string platform);
        Task SendNotificationToUser(Guid accountId, string title, string message, Dictionary<string, string> data = null);
        Task SendNotificationToTopic(string topic, string title, string message, Dictionary<string, string> data = null);
        Task SendNotificationToMultipleUsers(List<Guid> accountIds, string title, string message, Dictionary<string, string> data = null);
    }
}
