using DomainEventServices;
using Microsoft.Extensions.Logging;
using ProductCatalog.Services;
using System.Collections.Generic;
using System.Threading;

namespace ProductCatalog.Models
{
	public class CatalogModel : ICatalogModel
	{
		private readonly ICatalogStorage storage;
		private readonly IDomainEventDispatcher dispatcher;
		private readonly ILogger<CatalogModel> logger;

		public CatalogModel(ICatalogStorage storage, IDomainEventDispatcher dispatcher, ILogger<CatalogModel> logger)
		{
			this.storage = storage;
			this.dispatcher = dispatcher;
			this.logger = logger;
			logger.LogDebug("CatalogModel создан");
		}

		public int CountCategories(CancellationToken token = default) => storage.CountCategories(token);

		public bool HasAnyCategories(CancellationToken token = default) => storage.HasAnyCategories(token);

		public Category GetCategory(int categoryId, CancellationToken token = default) => storage.GetCategory(categoryId, token);

		public IEnumerable<Category> GetAllCategories(CancellationToken token = default) => storage.GetAllCategories(token);

		public void AddCategory(Category newData, CancellationToken token = default)
		{
			logger.LogTrace("CatalogModel: добавление категории {@newData}", newData);
			storage.AddCategory(newData, token);
			dispatcher.Raise(new CatalogAddCategoryEvent(newData));
		}

		public void UpdateCategory(Category newData, CancellationToken token = default)
		{
			logger.LogTrace("CatalogModel: изменение категории {@newData}", newData);
			storage.UpdateCategory(newData, token);
			dispatcher.Raise(new CatalogUpdateCategoryEvent(newData));
		}

		public void DeleteCategory(int categoryId, CancellationToken token = default)
		{
			logger.LogTrace("CatalogModel: удаление категории {CategoryId}", categoryId);
			storage.DeleteCategory(categoryId, token);
			dispatcher.Raise(new CatalogDeleteCategoryEvent(categoryId));
		}

		public int CountProducts(int categoryId, CancellationToken token = default) => storage.CountProducts(categoryId, token);

		public bool HasAnyProducts(int categoryId, CancellationToken token = default) => storage.HasAnyProducts(categoryId, token);

		public Product GetProduct(int categoryId, int productId, CancellationToken token = default) => storage.GetProduct(categoryId, productId, token);

		public IEnumerable<Product> GetAllProducts(int categoryId, CancellationToken token = default) => storage.GetAllProducts(categoryId, token);

		public void AddProduct(int categoryId, Product newData, CancellationToken token = default)
		{
			logger.LogTrace("CatalogModel: добавление продукта {@newData} в категорию {CategoryId}", newData, categoryId);
			storage.AddProduct(categoryId, newData, token);
			dispatcher.Raise(new CatalogAddProductEvent(categoryId, newData));
		}

		public void UpdateProduct(int categoryId, Product newData, CancellationToken token = default)
		{
			logger.LogTrace("CatalogModel: изменение продукта {@newData} в категории {CategoryId}", newData, categoryId);
			storage.UpdateProduct(categoryId, newData, token);
			dispatcher.Raise(new CatalogUpdateProductEvent(categoryId, newData));
		}

		public void DeleteProduct(int categoryId, int productId, CancellationToken token = default)
		{
			logger.LogTrace("CatalogModel: удаление продукта {ProductId} из категории {CategoryId}", productId, categoryId);
			storage.DeleteProduct(categoryId, productId, token);
			dispatcher.Raise(new CatalogDeleteProductEvent(categoryId, productId));
		}
	}
}
