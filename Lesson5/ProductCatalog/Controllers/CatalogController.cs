using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProductCatalog.Models;
using ProductCatalog.Services;
using System;

namespace ProductCatalog.Controllers
{
	public class CatalogController : Controller
	{
		private readonly ILogger<CatalogController> logger;
		private readonly ICatalogModel catalog;
		private readonly IMailNotifier notifier;

		private void SendNotification(string message)
		{
			try
			{
				notifier.SendNotification(message);
			} catch (Exception e)
			{
				logger.LogError(e, "не удалось отправить оповещение.");
			}
		}

		public CatalogController(ILogger<CatalogController> logger, ICatalogModel catalog, IMailNotifier notifier)
		{
			this.logger = logger;
			this.notifier = notifier;
			this.catalog = catalog;
			logger.LogInformation("Контроллер создан.");
		}

		[HttpGet("catalog/categories")]
		public IActionResult Categories()
		{
			return View(catalog);
		}

		[HttpPost("catalog/categories")]
		public IActionResult AddCategory([FromForm] CategoryAddingModel model)
		{
			if (!ModelState.IsValid)
				ViewData["Error"] = "Неверные данные";
			else
			{
				try
				{
					catalog.AddCategory(new Category(model.Id) { Name = model.Name });
				} catch (CatalogException e)
				{
					ViewData["Error"] = e.Message;
					logger.LogWarning("Ошибка при создании категории: {ErrorMessage}", e.Message);
				} catch (Exception e)
				{
					ViewData["Error"] = "Внутренняя ошибка сервера при обработке запроса, администратор оповещен";
					logger.LogError(e, "исключение при обработке запроса AddCategory {@model}", model);
					SendNotification($"Исключение {e.Message} при обработке запроса AddCategory {model.Id}");
				}
			}
			return View("Categories", catalog);
		}

		[HttpGet("catalog/deletecategory")]
		public IActionResult DeleteCategory(int categoryId)
		{
			try
			{
				catalog.DeleteCategory(categoryId);
			} catch (CatalogException e)
			{
				ViewData["Error"] = e.Message;
				logger.LogWarning("Ошибка при удалении категории: {ErrorMessage}", e.Message);
			} catch (Exception e)
			{
				ViewData["Error"] = "Внутренняя ошибка сервера при обработке запроса, администратор оповещен";
				logger.LogError(e, "исключение при обработке запроса DeleteCategory {CategoryId}", categoryId);
				SendNotification($"Исключение {e.Message} при обработке запроса DeleteCategory {categoryId}");
			}
			return View();
		}

		private CategoryViewData MakeCategoryViewData(int categoryId)
		{
			try
			{
				Category c = catalog.GetCategory(categoryId);
				return new CategoryViewData(categoryId, catalog) { Name = c.Name };
			} catch (CatalogException e)
			{
				logger.LogWarning("Категория {CategoryId} не найдена: {ErrorMessage}", categoryId, e.Message);
			} catch (Exception e)
			{
				logger.LogError(e, "исключение при получении категории {CategoryId}", categoryId);
				SendNotification($"Исключение {e.Message} при получении категории {categoryId}");
			}
			return null;
		}

		[HttpGet("catalog/products")]
		public IActionResult Products(int categoryId)
		{
			return View(MakeCategoryViewData(categoryId));
		}

		[HttpPost("catalog/products")]
		public IActionResult AddProduct(int categoryId, [FromForm] ProductAddingModel model)
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
					});
				} catch (CatalogException e)
				{
					ViewData["Error"] = e.Message;
					logger.LogWarning("Ошибка при добавлении продукта: {ErrorMessage}", e.Message);
				} catch (Exception e)
				{
					ViewData["Error"] = "Внутренняя ошибка сервера при обработке запроса, администратор оповещен";
					logger.LogError(e, "исключение при обработке запроса AddProduct {CategoryId}, {@model}", categoryId, model);
					SendNotification($"Исключение {e.Message} при обработке запроса AddProduct {categoryId}, {model.Id}");
				}
			}
			return View("Products", MakeCategoryViewData(categoryId));
		}

		[HttpGet("catalog/deleteproduct")]
		public IActionResult DeleteProduct(int categoryId, int productId)
		{
			try
			{
				catalog.DeleteProduct(categoryId, productId);
			} catch (CatalogException e)
			{
				ViewData["Error"] = e.Message;
				logger.LogWarning("Ошибка при удалении продукта: {ErrorMessage}", e.Message);
			} catch (Exception e)
			{
				ViewData["Error"] = "Внутренняя ошибка сервера при обработке запроса, администратор оповещен";
				logger.LogError(e, "исключение при обработке запроса DeleteProduct {CategoryId}, {ProductId}", categoryId, productId);
				SendNotification($"Исключение {e.Message} при обработке запроса DeleteProduct {categoryId}, {productId}");
			}
			return View(MakeCategoryViewData(categoryId));
		}
	}
}
