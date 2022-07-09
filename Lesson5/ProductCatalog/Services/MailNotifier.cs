using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Polly;
using Polly.Retry;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductCatalog.Services
{
	public class SmtpCredentials
	{
		public string Host { get; set; }
		public int Port { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public string SendTo { get; set; }
	}

	public class MailNotifier : IMailNotifier
	{
		private readonly ILogger<MailNotifier> logger;
		private readonly SmtpCredentials credentials;

		public MailNotifier(IOptions<SmtpCredentials> options, ILogger<MailNotifier> logger)
		{
			credentials = options.Value;
			this.logger = logger;
			logger.LogInformation("Создан с параметрами {@credentials}", credentials);
		}

		private async Task TrySendMessageAsync(MimeMessage message, CancellationToken token)
		{
			logger.LogTrace("Попытка отправки сообщения");
			using var client = new SmtpClient();
			await client.ConnectAsync(credentials.Host, credentials.Port, false, token);
			await client.AuthenticateAsync(credentials.UserName, credentials.Password, token);
			await client.SendAsync(message, token);
			await client.DisconnectAsync(true, token);
		}

		public async Task SendNotificationAsync(string message, CancellationToken token = default)
		{
			logger.LogInformation("Отправка сообщения '{Message}'", message);
			var emailMessage = new MimeMessage();
			emailMessage.From.Add(new MailboxAddress("Робот каталога", credentials.UserName));
			emailMessage.To.Add(new MailboxAddress("Администратор сайта", credentials.SendTo));
			emailMessage.Subject = "Изменения в каталоге";
			emailMessage.Body = new TextPart("Plain") { Text = message };

			AsyncRetryPolicy? policy = Policy
				.Handle<Exception>()
				.RetryAsync(3, onRetry: (e, r) =>
				{
					logger.LogWarning(e, "Ошибка при отправке email, попытка {Attempt}", r);
				});
			PolicyResult? result = await policy.ExecuteAndCaptureAsync(t => TrySendMessageAsync(emailMessage, t), token);
			if (result.Outcome == OutcomeType.Failure)
			{
				logger.LogError(result.FinalException, "Отправить email не удалось.");
			}
		}

		public void SendNotification(string message)
		{
			Task t = SendNotificationAsync(message);
			t.Wait();
		}
	}
}
