using DomainEventServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductCatalog.Models;
using System;

namespace ProductCatalog.Controllers
{
	public class ExceptionFilter : IExceptionFilter
	{
		private readonly IHostEnvironment hostEnvironment;
		private readonly ILogger<ExceptionFilter> logger;
		private readonly IDomainEventDispatcher dispatcher;

		public ExceptionFilter(IHostEnvironment hostEnvironment, ILogger<ExceptionFilter> logger, IDomainEventDispatcher dispatcher)
		{
			this.hostEnvironment = hostEnvironment;
			this.logger = logger;
			this.dispatcher = dispatcher;
			logger.LogInformation("ExceptionFilter создан.");
		}

		public void OnException(ExceptionContext context)
		{
			context.ExceptionHandled = true;
			if (context.Exception is OperationCanceledException)
			{
				context.Result = new ContentResult { Content = "Операция прервана" };
				return;
			}
			string request = context.RouteData.Values["action"].ToString();

			logger.LogError(context.Exception, "ExceptionFilter: исключение в CatalogController при обработке запроса {Request}", request);
			string notification = $"Исключение {context.Exception.Message} при обработке запроса {request}";
			if (hostEnvironment.IsDevelopment())
			{
				context.Result = new ContentResult { Content = notification };
			} else
			{
				try
				{
					dispatcher.Raise(new CatalogErrorEvent(notification, context.Exception));
				} catch (Exception e)
				{
					logger.LogError(e, "ExceptionFilter: не удалось отправить оповещение.");
				}
				context.Result = new ContentResult { Content = "Внутренняя ошибка сервера при обработке запроса, администратор оповещен" };
			}
		}
	}
}
