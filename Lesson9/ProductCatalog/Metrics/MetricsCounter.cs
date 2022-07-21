using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace SiteMetrics
{
	public class MetricsCounter
	{
		private readonly RequestDelegate next;
		private readonly IMetricsStorage storage;
		private readonly ILogger<MetricsCounter> logger;

		public MetricsCounter(RequestDelegate next, IMetricsStorage storage, ILogger<MetricsCounter> logger)
		{
			this.next = next;
			this.storage = storage;
			this.logger = logger;
			logger.LogInformation("MetricsCounter создан.");
		}

		public async Task InvokeAsync(HttpContext context)
		{
			int Count = storage.Increment(context.Request.Path);
			logger.LogInformation("MetricsCounter: подсчет {Path}, сейчас {PathCount}.", context.Request.Path, Count);
			await next(context);
		}
	}
}
