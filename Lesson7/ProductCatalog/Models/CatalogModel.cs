using Microsoft.Extensions.Logging;
using ProductCatalog.Services;
using System.Collections.Generic;
using System.Threading;

namespace ProductCatalog.Models
{
	public class CatalogModel : ICatalogModel
	{
		private readonly ICatalogStorage storage;
		private readonly INotificationDispatcher notifier;
		private readonly ILogger<CatalogModel> logger;

		public CatalogModel(ICatalogStorage storage, INotificationDispatcher notifier, ILogger<CatalogModel> logger)
		{
			this.storage = storage;
			this.notifier = notifier;
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
			notifier.EnqueueCatalogEventNotification($"В каталоге добавлена новая категория: Id = {newData.Id}, Name = {newData.Name}.");
		}

		public void UpdateCategory(Category newData, CancellationToken token = default)
		{
			logger.LogTrace("CatalogModel: изменение категории {@newData}", newData);
			storage.UpdateCategory(newData, token);
			notifier.EnqueueCatalogEventNotification($"В каталоге изменена категория: Id = {newData.Id}, Name = {newData.Name}.");
		}

		public void DeleteCategory(int categoryId, CancellationToken token = default)
		{
			logger.LogTrace("CatalogModel: удаление категории {CategoryId}", categoryId);
			storage.DeleteCategory(categoryId, token);
			notifier.EnqueueCatalogEventNotification($"В каталоге удалена категория: Id = {categoryId}.");
		}

		public int CountProducts(int categoryId, CancellationToken token = default) => storage.CountProducts(categoryId, token);

		public bool HasAnyProducts(int categoryId, CancellationToken token = default) => storage.HasAnyProducts(categoryId, token);

		public Product GetProduct(int categoryId, int productId, CancellationToken token = default) => storage.GetProduct(categoryId, productId, token);

		public IEnumerable<Product> GetAllProducts(int categoryId, CancellationToken token = default) => storage.GetAllProducts(categoryId, token);

		public void AddProduct(int categoryId, Product newData, CancellationToken token = default)
		{
			logger.LogTrace("CatalogModel: добавление продукта {@newData} в категорию {CategoryId}", newData, categoryId);
			storage.AddProduct(categoryId, newData, token);
			notifier.EnqueueCatalogEventNotification($"В каталоге в категорию {categoryId} добавлен новый продукт: Id = {newData.Id}, Name = {newData.Name}.");
		}

		public void UpdateProduct(int categoryId, Product newData, CancellationToken token = default)
		{
			logger.LogTrace("CatalogModel: изменение продукта {@newData} в категории {CategoryId}", newData, categoryId);
			storage.UpdateProduct(categoryId, newData, token);
			notifier.EnqueueCatalogEventNotification($"В каталоге в категории {categoryId} изменен продукт: Id = {newData.Id}, Name = {newData.Name}.");
		}

		public void DeleteProduct(int categoryId, int productId, CancellationToken token = default)
		{
			logger.LogTrace("CatalogModel: удаление продукта {ProductId} из категории {CategoryId}", productId, categoryId);
			storage.DeleteProduct(categoryId, productId, token);
			notifier.EnqueueCatalogEventNotification($"В каталоге из категории {categoryId} удален продукт: Id = {productId}.");
		}
	}
}
