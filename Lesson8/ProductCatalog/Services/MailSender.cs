using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Polly;
using Polly.Retry;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MailServices
{
	public class SmtpCredentials
	{
		public string Host { get; set; }
		public int Port { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
	}

	public class MailSender : IMailSender
	{
		private readonly ILogger<MailSender> logger;
		private readonly SmtpCredentials credentials;

		public MailSender(IOptions<SmtpCredentials> options, ILogger<MailSender> logger)
		{
			this.logger = logger;
			credentials = options.Value;
			if (credentials.Host == null) throw new Exception("В параметрах SmtpCredentials не задан Host");
			if (credentials.Port == 0) throw new Exception("В параметрах SmtpCredentials не задан Port");
			if (credentials.UserName == null) throw new Exception("В параметрах SmtpCredentials не задан UserName");
			if (credentials.Password == null) throw new Exception("В параметрах SmtpCredentials не задан Password");
			logger.LogInformation("MailSender создан с параметрами {@credentials}", credentials);
		}

		public async Task SendMessageAsync(string address, string to, string from, string subject, string text, CancellationToken token = default)
		{
			logger.LogInformation("MailSender: Отправка сообщения '{Message}'", text);
			var message = new MimeMessage();
			message.From.Add(new MailboxAddress(from, credentials.UserName));
			message.To.Add(new MailboxAddress(to, address));
			message.Subject = subject;
			message.Body = new TextPart("Plain") { Text = text };
			await SendMessageAsync(message, token);
		}

		public async Task SendMessageAsync(MimeMessage message, CancellationToken token = default)
		{
			logger.LogInformation("MailSender: Отправка сообщения", message);
			AsyncRetryPolicy policy = Policy
				.Handle<Exception>()
				.RetryAsync(3, onRetry: (e, r) =>
				{
					logger.LogWarning(e, "MailSender: ошибка при отправке email, попытка {Attempt}", r);
				});
			PolicyResult result = await policy.ExecuteAndCaptureAsync(t => TrySendMessageAsync(message, t), token);
			if (result.Outcome == OutcomeType.Failure)
			{
				logger.LogError(result.FinalException, "MailSender: отправить email не удалось.");
				throw new Exception("Отправить email не удалось.", result.FinalException);
			}
		}

		private async Task TrySendMessageAsync(MimeMessage message, CancellationToken token)
		{
			logger.LogTrace("MailSender: Попытка отправки сообщения");
			using var client = new SmtpClient();
			await client.ConnectAsync(credentials.Host, credentials.Port, false, token);
			await client.AuthenticateAsync(credentials.UserName, credentials.Password, token);
			await client.SendAsync(message, token);
			await client.DisconnectAsync(true, token);
		}
	}
}
