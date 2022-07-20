namespace ProductCatalog.Services
{
	public interface INotificationDispatcher {
		public void SendErrorNotficiation(string message);
		public void EnqueueCatalogEventNotification(string message);
	}
}
