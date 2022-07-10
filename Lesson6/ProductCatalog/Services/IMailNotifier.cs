using System.Threading;
using System.Threading.Tasks;

namespace ProductCatalog.Services
{
	public interface IMailNotifier
	{
		public Task SendNotificationAsync(string message, CancellationToken token = default);
		public void SendUrgentNotification(string message);
		public void SendNotification(string message);
	}
}
