using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProductCatalog.Models;
using ProductCatalog.Services;
using System;
using System.Threading;

namespace ProductCatalog.Controllers
{
	public class CatalogController : Controller
	{
		private readonly ILogger<CatalogController> logger;
		private readonly ICatalogModel catalog;
		private readonly INotificationDispatcher notifier;

		private void SendNotification(string message)
		{
			try
			{
				notifier.SendErrorNotficiation(message);
			} catch (Exception e)
			{
				logger.LogError(e, "CatalogController: не удалось отправить оповещение.");
			}
		}

		public CatalogController(ILogger<CatalogController> logger, ICatalogModel catalog, INotificationDispatcher notifier)
		{
			this.logger = logger;
			this.notifier = notifier;
			this.catalog = catalog;
			logger.LogInformation("CatalogController создан.");
		}

		[HttpGet("catalog/categories")]
		public IActionResult Categories(CancellationToken token)
		{
			return View(catalog);
		}

		[HttpPost("catalog/categories")]
		public IActionResult AddCategory([FromForm] CategoryAddingModel model, CancellationToken token)
		{
			if (!ModelState.IsValid)
				ViewData["Error"] = "Неверные данные";
			else
			{
				try
				{
					catalog.AddCategory(new Category(model.Id) { Name = model.Name }, token);
				} catch (CatalogException e)
				{
					ViewData["Error"] = e.Message;
					logger.LogWarning("CatalogController: ошибка при создании категории: {ErrorMessage}", e.Message);
				} catch (OperationCanceledException e) {
					throw e;
				} catch (Exception e)
				{
					ViewData["Error"] = "Внутренняя ошибка сервера при обработке запроса, администратор оповещен";
					logger.LogError(e, "CatalogController: исключение при обработке запроса AddCategory {@model}", model);
					SendNotification($"Исключение {e.Message} при обработке запроса AddCategory {model.Id}");
				}
			}
			return View("Categories", catalog);
		}

		[HttpGet("catalog/deletecategory")]
		public IActionResult DeleteCategory(int categoryId, CancellationToken token)
		{
			try
			{
				catalog.DeleteCategory(categoryId, token);
			} catch (CatalogException e)
			{
				ViewData["Error"] = e.Message;
				logger.LogWarning("CatalogController: ошибка при удалении категории: {ErrorMessage}", e.Message);
			} catch (OperationCanceledException e)
			{
				throw e;
			} catch (Exception e)
			{
				ViewData["Error"] = "Внутренняя ошибка сервера при обработке запроса, администратор оповещен";
				logger.LogError(e, "CatalogController: исключение при обработке запроса DeleteCategory {CategoryId}", categoryId);
				SendNotification($"Исключение {e.Message} при обработке запроса DeleteCategory {categoryId}");
			}
			return View();
		}

		private CategoryViewData MakeCategoryViewData(int categoryId, CancellationToken token)
		{
			try
			{
				Category c = catalog.GetCategory(categoryId, token);
				return new CategoryViewData(categoryId, catalog) { Name = c.Name };
			} catch (CatalogException e)
			{
				logger.LogWarning("CatalogController: категория {CategoryId} не найдена: {ErrorMessage}", categoryId, e.Message);
			} catch (OperationCanceledException e)
			{
				throw e;
			} catch (Exception e)
			{
				logger.LogError(e, "CatalogController: исключение при получении категории {CategoryId}", categoryId);
				SendNotification($"Исключение {e.Message} при получении категории {categoryId}");
			}
			return null;
		}

		[HttpGet("catalog/products")]
		public IActionResult Products(int categoryId, CancellationToken token)
		{
			return View(MakeCategoryViewData(categoryId, token));
		}

		[HttpPost("catalog/products")]
		public IActionResult AddProduct(int categoryId, [FromForm] ProductAddingModel model, CancellationToken token)
		{
			if (!ModelState.IsValid)
				ViewData["Error"] = "Неверные данные";
			else
			{
				try
				{
					catalog.AddProduct(categoryId, new Product(model.Id)
					{
						Name = model.Name,
						ImgUrl = model.ImgUrl,
						Price = model.Price
					}, token);
				} catch (CatalogException e)
				{
					ViewData["Error"] = e.Message;
					logger.LogWarning("CatalogController: ошибка при добавлении продукта: {ErrorMessage}", e.Message);
				} catch (OperationCanceledException e)
				{
					throw e;
				} catch (Exception e)
				{
					ViewData["Error"] = "Внутренняя ошибка сервера при обработке запроса, администратор оповещен";
					logger.LogError(e, "CatalogController: исключение при обработке запроса AddProduct {CategoryId}, {@model}", categoryId, model);
					SendNotification($"Исключение {e.Message} при обработке запроса AddProduct {categoryId}, {model.Id}");
				}
			}
			return View("Products", MakeCategoryViewData(categoryId, token));
		}

		[HttpGet("catalog/deleteproduct")]
		public IActionResult DeleteProduct(int categoryId, int productId, CancellationToken token)
		{
			try
			{
				catalog.DeleteProduct(categoryId, productId, token);
			} catch (CatalogException e)
			{
				ViewData["Error"] = e.Message;
				logger.LogWarning("CatalogController: ошибка при удалении продукта: {ErrorMessage}", e.Message);
			} catch (OperationCanceledException e)
			{
				throw e;
			} catch (Exception e)
			{
				ViewData["Error"] = "Внутренняя ошибка сервера при обработке запроса, администратор оповещен";
				logger.LogError(e, "CatalogController: исключение при обработке запроса DeleteProduct {CategoryId}, {ProductId}", categoryId, productId);
				SendNotification($"Исключение {e.Message} при обработке запроса DeleteProduct {categoryId}, {productId}");
			}
			return View(MakeCategoryViewData(categoryId, token));
		}
	}
}
