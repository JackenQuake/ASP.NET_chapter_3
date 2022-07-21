using MimeKit;
using System.Threading;
using System.Threading.Tasks;

namespace MailServices
{
	public interface IMailSender
	{
		public Task SendMessageAsync(string address, string to, string from, string subject, string text, CancellationToken token = default);
		public Task SendMessageAsync(MimeMessage message, CancellationToken token = default);
	}
}
