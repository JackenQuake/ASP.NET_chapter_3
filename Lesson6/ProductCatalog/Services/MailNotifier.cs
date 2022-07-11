using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Concurrent;
using System.Text;
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

		private class NotificationRecord
		{
			public string Message { get; set; }
			public DateTime Timestamp { get; set; }
		}

		private ConcurrentQueue<NotificationRecord> notificationQueue;

		public MailNotifier(IOptions<SmtpCredentials> options, ILogger<MailNotifier> logger)
		{
			this.logger = logger;
			credentials = options.Value;
			notificationQueue = new ConcurrentQueue<NotificationRecord>();
			_ = NotificationQueueProcessorTask();
			logger.LogInformation("MailNotifier создан с параметрами {@credentials}", credentials);
		}

		public void SendNotification(string message)
		{
			notificationQueue.Enqueue(new NotificationRecord() { Message = message, Timestamp = DateTime.Now });
		}

		private async Task NotificationQueueProcessorTask()
		{
			while (true)
			{
				await Task.Delay(300000);
				StringBuilder sb = null;
				NotificationRecord result;
				while (notificationQueue.TryDequeue(out result))
				{
					// Для экономии ресурсов StringBuilder будет создан только если в очереди хоть что-то есть
					if (sb == null) sb = new StringBuilder();
					sb.Append(result.Timestamp.ToString());
					sb.Append(": ");
					sb.Append(result.Message);
					sb.Append("\n");
				}
				// ... и если StringBuilder не был создан - очередь была пуста и мы сразу выходим
				if (sb == null) continue;
				logger.LogInformation("MailNotifier: периодическая отправка сообщений");
				await SendNotificationAsync(sb.ToString());
			}
		}

		public void SendUrgentNotification(string message)
		{
			_ = SendNotificationAsync(message);
		}

		public async Task SendNotificationAsync(string message, CancellationToken token = default)
		{
			logger.LogInformation("MailNotifier: Отправка сообщения '{Message}'", message);
			var emailMessage = new MimeMessage();
			emailMessage.From.Add(new MailboxAddress("Робот каталога", credentials.UserName));
			emailMessage.To.Add(new MailboxAddress("Администратор сайта", credentials.SendTo));
			emailMessage.Subject = "Событие в каталоге продуктов";
			emailMessage.Body = new TextPart("Plain") { Text = message };
			AsyncRetryPolicy policy = Policy
				.Handle<Exception>()
				.RetryAsync(3, onRetry: (e, r) =>
				{
					logger.LogWarning(e, "MailNotifier: ошибка при отправке email, попытка {Attempt}", r);
				});
			PolicyResult result = await policy.ExecuteAndCaptureAsync(t => TrySendMessageAsync(emailMessage, t), token);
			if (result.Outcome == OutcomeType.Failure)
			{
				logger.LogError(result.FinalException, "MailNotifier: отправить email не удалось.");
			}
		}

		private async Task TrySendMessageAsync(MimeMessage message, CancellationToken token)
		{
			logger.LogTrace("MailNotifier: Попытка отправки сообщения");
			using var client = new SmtpClient();
			await client.ConnectAsync(credentials.Host, credentials.Port, false, token);
			await client.AuthenticateAsync(credentials.UserName, credentials.Password, token);
			await client.SendAsync(message, token);
			await client.DisconnectAsync(true, token);
		}
	}
}
