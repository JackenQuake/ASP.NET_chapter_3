using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProductCatalog.Models;

namespace ProductCatalog.Controllers
{
	public class CatalogController : Controller
	{
		private readonly ILogger<CatalogController> _logger;
		private readonly Catalog catalog;

		public CatalogController(ILogger<CatalogController> logger)
		{
			_logger = logger;
			catalog = Catalog.GetCatalog();
		}

		[HttpGet("catalog/products")]
		public IActionResult Products()
		{
			return View(catalog);
		}

		[HttpPost("catalog/products")]
		public IActionResult AddCategory([FromForm] CategoryAddingModel model)
		{
			string errMsg;
			if (!ModelState.IsValid)
				errMsg = "Неверные данные";
			else errMsg = catalog.AddCategory(new Category()
			{
				Id = model.Id,
				Name = model.Name
			});
			if (errMsg != null) ViewData["Error"] = errMsg;
			return View("Products", catalog);
		}

		[HttpGet("catalog/category")]
		public IActionResult Category(int categoryId)
		{
			return View(catalog.GetCategory(categoryId));
		}

		[HttpPost("catalog/category")]
		public IActionResult AddProduct(int categoryId, [FromForm] ProductAddingModel model)
		{
			string errMsg;
			if (!ModelState.IsValid)
				errMsg = "Неверные данные";
			else errMsg = catalog.GetCategory(categoryId).AddProduct(new Product()
			{
				Id = model.Id,
				Name = model.Name,
				ImgUrl = model.ImgUrl,
				Price = model.Price
			});
			if (errMsg != null) ViewData["Error"] = errMsg;
			return View("Category", catalog.GetCategory(categoryId));
		}

	}
}
