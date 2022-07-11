namespace ProductCatalog.Services
{
	public interface IMailNotifier
	{
		public void SendNotification(string message);
	}
}
