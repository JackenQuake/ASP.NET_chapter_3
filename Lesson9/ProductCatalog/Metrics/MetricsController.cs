using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SiteMetrics
{
	public class MetricsController : Controller
	{
		private readonly IMetricsStorage storage;
		private readonly ILogger<MetricsController> logger;

		public MetricsController(IMetricsStorage storage, ILogger<MetricsController> logger)
		{
			this.storage = storage;
			this.logger = logger;
			logger.LogInformation("MetricsController создан.");
		}

		[HttpGet("metrics")]
		public IActionResult Index()
		{
			logger.LogInformation("MetricsController: просмотр статистики.");
			return View(storage);
		}
	}
}
