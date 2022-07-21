using DomainEventServices;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProductCatalog.Models;
using System;
using System.Text;

namespace ProductCatalog.Controllers
{
	public class HomeController : Controller
	{
		private readonly IDomainEventDispatcher dispatcher;
		private readonly ILogger<HomeController> logger;

		public HomeController(IDomainEventDispatcher dispatcher, ILogger<HomeController> logger)
		{
			this.dispatcher = dispatcher;
			this.logger = logger;
			logger.LogInformation("HomeController создан.");
		}

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
			if (exceptionHandlerPathFeature == null) return Ok();
			Exception err = exceptionHandlerPathFeature.Error;
			if (err is OperationCanceledException) return Ok();
			StringBuilder request = new();
			request.Append(exceptionHandlerPathFeature.Path);
			try
			{
				foreach (var x in HttpContext.Request.Query) request.Append($", Query: {x.Key} = {x.Value}");
			} catch (Exception) { }
			try
			{
				foreach (var x in HttpContext.Request.Form)
					request.Append($", Form: {x.Key} = {x.Value}");
			} catch (Exception) { }
			logger.LogError(err, "Error: исключение при обработке запроса {Request}", request.ToString());
			string notification = $"Исключение {err.Message} при обработке запроса " + request.ToString();
			try
			{
				dispatcher.Raise(new CatalogErrorEvent(notification, err));
			} catch (Exception e)
			{
				logger.LogError(e, "Error: не удалось отправить оповещение.");
			}
			return View();
		}
	}
}
