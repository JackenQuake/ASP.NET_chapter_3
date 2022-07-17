using MailServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProductCatalog.Services
{
	public class NotificationSettings
	{
		public string AdminAddress { get; set; }
		public string AdminName { get; set; }
	}

	public class NotificationDispatcher : BackgroundService, IAsyncDisposable, INotificationDispatcher
	{
		private class NotificationRecord
		{
			public string Message { get; set; }
			public DateTime Timestamp { get; set; }
		}

		private readonly IMailSender mailer;
		private readonly ILogger<NotificationDispatcher> logger;
		private readonly ConcurrentQueue<NotificationRecord> notificationQueue;
		private readonly NotificationSettings settings;
		private int hourCounter = 0;

		public NotificationDispatcher(IOptions<NotificationSettings> options, ILogger<NotificationDispatcher> logger, IMailSender mailer)
		{
			this.logger = logger;
			this.mailer = mailer;
			settings = options.Value;
			logger.LogInformation("NotificationDispatcher создан с параметрами {@NotificationSettings}", settings);
			notificationQueue = new ConcurrentQueue<NotificationRecord>();
		}

		public void SendErrorNotficiation(string message)
		{
			_ = mailer.SendMessageAsync(settings.AdminAddress, settings.AdminName, "Робот каталога", "Ошибка в каталоге продуктов", message);
		}

		public void EnqueueCatalogEventNotification(string message)
		{
			notificationQueue.Enqueue(new NotificationRecord() { Message = message, Timestamp = DateTime.Now });
		}

		private async Task SendQueuedNotifications(bool periodicNotifications, CancellationToken token = default)
		{
			logger.LogDebug("NotificationDispatcher: проверка очереди");
			// Проверим, есть ли что-то в очереди, и создадим StringBuilder со всеми имеюшимися сообщениями
			StringBuilder sb = null;
			if (periodicNotifications)  // Оповещения каждый час
			{
				hourCounter++;
				if (hourCounter == 12)
				{
					hourCounter = 0;
					sb = new StringBuilder();
					sb.Append("Оповещение каждый час: сервер работает");
				}
			}
			// Теперь проверим очередь
			while (notificationQueue.TryDequeue(out NotificationRecord result))
			{
				// Для экономии ресурсов StringBuilder будет создан только если в очереди хоть что-то есть
				if (sb == null) sb = new StringBuilder(); else sb.Append(Environment.NewLine);
				sb.Append(result.Timestamp.ToString());
				sb.Append(": ");
				sb.Append(result.Message);
			}
			// ... если StringBuilder не был создан - очередь сообщений пуста
			if (sb == null) return;
			logger.LogInformation("NotificationDispatcher: отправка сообщений из очереди");
			await mailer.SendMessageAsync(settings.AdminAddress, settings.AdminName, "Робот каталога", "Оповещение каталога продуктов", sb.ToString(), token);
		}

		protected override async Task ExecuteAsync(CancellationToken token)
		{
			using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
			while (!token.IsCancellationRequested)
			{
				// Ждем 5 минут, но с проверкой CancellationToken: если будет запрошено прерывание, мы прервем ожидание
				try
				{
					await timer.WaitForNextTickAsync(token);
				} catch (TaskCanceledException) { }
				// Заметим, что если CencellationToken и запросил отмену - письма мы все равно должны отправить, так что в SendQueuedNotifications мы наш токен не отдаем
				await SendQueuedNotifications(!token.IsCancellationRequested);
			}
		}

		public async ValueTask DisposeAsync()
		{
			await SendQueuedNotifications(false);
		}
	}
}
