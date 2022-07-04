using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProductCatalog.Models;

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
		}

		[HttpGet("catalog/categories")]
		public IActionResult Categories()
		{
			return View(catalog);
		}

		[HttpPost("catalog/categories")]
		public IActionResult AddCategory([FromForm] CategoryAddingModel model)
		{
			string errMsg;
			if (!ModelState.IsValid)
				errMsg = "Неверные данные";
			else errMsg = catalog.AddCategory(new Category(model.Id) { Name = model.Name });
			if (errMsg != null) ViewData["Error"] = errMsg;
			return View("Categories", catalog);
		}

		[HttpGet("catalog/deletecategory")]
		public IActionResult DeleteCategory(int categoryId)
		{
			catalog.DeleteCategory(categoryId);
			return View();
		}

		private CategoryViewData MakeCategoryViewData(int categoryId)
		{
			Category c = catalog.GetCategory(categoryId);
			return (c == null) ? null : new CategoryViewData(categoryId, catalog) { Name = c.Name };
		}

		[HttpGet("catalog/products")]
		public IActionResult Products(int categoryId)
		{
			return View(MakeCategoryViewData(categoryId));
		}

		[HttpPost("catalog/products")]
		public IActionResult AddProduct(int categoryId, [FromForm] ProductAddingModel model)
		{
			string errMsg;
			if (!ModelState.IsValid)
				errMsg = "Неверные данные";
			else errMsg = catalog.AddProduct(categoryId, new Product(model.Id)
			{
				Name = model.Name,
				ImgUrl = model.ImgUrl,
				Price = model.Price
			});
			if (errMsg != null) ViewData["Error"] = errMsg;
			return View("Products", MakeCategoryViewData(categoryId));
		}

		[HttpGet("catalog/deleteproduct")]
		public IActionResult DeleteProduct(int categoryId, int productId)
		{
			catalog.DeleteProduct(categoryId, productId);
			return View(MakeCategoryViewData(categoryId));
		}
	}
}
