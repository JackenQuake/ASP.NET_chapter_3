using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProductCatalog.Models;
using System;
using System.Threading;

namespace ProductCatalog.Controllers
{
	public class CatalogController : Controller
	{
		private readonly ILogger<CatalogController> logger;
		private readonly ICatalogModel catalog;

		public CatalogController(ILogger<CatalogController> logger, ICatalogModel catalog)
		{
			this.logger = logger;
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
			}
			return View();
		}

		[HttpGet("catalog/products")]
		public IActionResult Products(int categoryId, CancellationToken token)
		{
			var data = MakeCategoryViewData(categoryId, token);
			return View(data);
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
				}
			}
			var data = MakeCategoryViewData(categoryId, token);
			return View("Products", data);
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
			}
			var data = MakeCategoryViewData(categoryId, token);
			return View(data);
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
			}
			return null;
		}
	}
}
