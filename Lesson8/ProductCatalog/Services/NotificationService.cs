using DomainEventServices;
using MailServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProductCatalog.Models;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProductCatalog.Services
{
	public enum NotificationFileMode
	{
		SaveAll = 0, SaveNotSended = 1, SaveNothing = 2
	}

	public class NotificationSettings
	{
		public string AdminAddress { get; set; }
		public string AdminName { get; set; }
		public NotificationFileMode NotificationFileMode { get; set; }
		public string NotificationFile { get; set; }
	}

	public class NotificationService : BackgroundService, IAsyncDisposable
	{
		private class NotificationRecord
		{
			public string Message { get; set; }
			public DateTime Timestamp { get; set; }
		}

		private readonly IMailSender mailer;
		private readonly ILogger<NotificationService> logger;
		private readonly ConcurrentQueue<NotificationRecord> notificationQueue;
		private readonly NotificationSettings settings;
		private int hourCounter = 0;

		public NotificationService(IOptions<NotificationSettings> options, ILogger<NotificationService> logger, IMailSender mailer, IDomainEventDispatcher dispatcher)
		{
			this.logger = logger;
			this.mailer = mailer;
			dispatcher.RegisterEventHandler(typeof(CatalogChangeEvent), EnqueueCatalogEventNotification);
			dispatcher.RegisterEventHandler(typeof(CatalogErrorEvent), SendErrorNotficiation);
			settings = options.Value;
			if (settings.AdminAddress == null) throw new Exception("В параметрах NotificationSettings не задан AdminAddress");
			if (settings.AdminName == null) throw new Exception("В параметрах NotificationSettings не задан AdminName");
			if ((settings.NotificationFile == null) && (settings.NotificationFileMode != NotificationFileMode.SaveNothing))
				throw new Exception("В параметрах NotificationSettings не задан NotificationFile");
			logger.LogInformation("NotificationService создан с параметрами {@NotificationSettings}", settings);
			notificationQueue = new ConcurrentQueue<NotificationRecord>();
		}

		private async Task SendNotificationAsync(string subject, string text, CancellationToken token = default)
		{
			bool SaveToFile = settings.NotificationFileMode == NotificationFileMode.SaveAll;
			try
			{
				await mailer.SendMessageAsync(settings.AdminAddress, settings.AdminName, "Робот каталога", subject, text, token);
			} catch (Exception e)
			{
				logger.LogWarning(e, "NotificationService: ошибка при отправке почты");
				SaveToFile |= (settings.NotificationFileMode == NotificationFileMode.SaveNotSended);
			}
			if (SaveToFile)
			{
				await File.AppendAllTextAsync(settings.NotificationFile, text, token);
				await File.AppendAllTextAsync(settings.NotificationFile, Environment.NewLine, token);
			}
		}

		public async void SendErrorNotficiation(DomainEvent e)
		{
			string message = (e as CatalogErrorEvent).Message;
			logger.LogInformation("NotificationService: сообщение об ошибке {ErrorMessage}", message);
			await SendNotificationAsync("Ошибка в каталоге продуктов", message);
		}

		public void EnqueueCatalogEventNotification(DomainEvent e)
		{
			string message = (e as CatalogChangeEvent).Message;
			logger.LogDebug("NotificationService: оповещение об изменении каталога {Notification}", message);
			notificationQueue.Enqueue(new NotificationRecord() { Message = message, Timestamp = DateTime.Now });
		}

		private async Task SendQueuedNotifications(bool periodicNotifications, CancellationToken token = default)
		{
			logger.LogDebug("NotificationService: проверка очереди");
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
			logger.LogInformation("NotificationService: отправка сообщений из очереди");
			await SendNotificationAsync("Оповещение каталога продуктов", sb.ToString(), token);
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
