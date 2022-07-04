using MailKit.Net.Smtp;
using MimeKit;

namespace ProductCatalog.Services
{
	public class MailNotifier : IMailNotifier
	{
		public MailNotifier() {	}

		public void SendNotification(string message)
		{
			var emailMessage = new MimeMessage();
			emailMessage.From.Add(new MailboxAddress("Робот каталога", "**********"));
			emailMessage.To.Add(new MailboxAddress("Администратор сайта", "**********"));
			emailMessage.Subject = "Изменения в каталоге";
			emailMessage.Body = new TextPart("Plain") { Text = message };
			using (var client = new SmtpClient())
			{
				client.Connect("**********", 25, false);
				client.Authenticate("**********", "**********");
				client.Send(emailMessage);
				client.Disconnect(true);
			}
		}
	}
}
