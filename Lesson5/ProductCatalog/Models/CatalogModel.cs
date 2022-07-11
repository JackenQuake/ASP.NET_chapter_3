using Microsoft.Extensions.Logging;
using ProductCatalog.Services;
using System.Collections.Generic;

namespace ProductCatalog.Models
{
	public class CatalogModel : ICatalogModel
	{
		private readonly ICatalogStorage storage;
		private readonly IMailNotifier notifier;
		private readonly ILogger<CatalogModel> logger;

		public CatalogModel(ICatalogStorage storage, IMailNotifier notifier, ILogger<CatalogModel> logger)
		{
			this.storage = storage;
			this.notifier = notifier;
			this.logger = logger;
			logger.LogDebug("Класс создан");
		}

		public int CountCategories() => storage.CountCategories();

		public bool HasAnyCategories() => storage.HasAnyCategories();

		public Category GetCategory(int categoryId) => storage.GetCategory(categoryId);

		public IEnumerable<Category> GetAllCategories() => storage.GetAllCategories();

		public void AddCategory(Category newData)
		{
			logger.LogTrace("Добавление категории {@newData}", newData);
			storage.AddCategory(newData);
			notifier.SendNotification($"В каталоге добавлена новая категория: Id = {newData.Id}, Name = {newData.Name}.");
		}

		public void UpdateCategory(Category newData)
		{
			logger.LogTrace("Изменение категории {@newData}", newData);
			storage.UpdateCategory(newData);
			notifier.SendNotification($"В каталоге изменена категория: Id = {newData.Id}, Name = {newData.Name}.");
		}

		public void DeleteCategory(int categoryId)
		{
			logger.LogTrace("Удаление категории {CategoryId}", categoryId);
			storage.DeleteCategory(categoryId);
			notifier.SendNotification($"В каталоге удалена категория: Id = {categoryId}.");
		}

		public int CountProducts(int categoryId) => storage.CountProducts(categoryId);

		public bool HasAnyProducts(int categoryId) => storage.HasAnyProducts(categoryId);

		public Product GetProduct(int categoryId, int productId) => storage.GetProduct(categoryId, productId);

		public IEnumerable<Product> GetAllProducts(int categoryId) => storage.GetAllProducts(categoryId);

		public void AddProduct(int categoryId, Product newData)
		{
			logger.LogTrace("Добавление продукта {@newData} в категорию {CategoryId}", newData, categoryId);
			storage.AddProduct(categoryId, newData);
			notifier.SendNotification($"В каталоге в категорию {categoryId} добавлен новый продукт: Id = {newData.Id}, Name = {newData.Name}.");
		}

		public void UpdateProduct(int categoryId, Product newData)
		{
			logger.LogTrace("Изменение продукта {@newData} в категории {CategoryId}", newData, categoryId);
			storage.UpdateProduct(categoryId, newData);
			notifier.SendNotification($"В каталоге в категории {categoryId} изменен продукт: Id = {newData.Id}, Name = {newData.Name}.");
		}

		public void DeleteProduct(int categoryId, int productId)
		{
			logger.LogTrace("Удаление продукта {ProductId} из категории {CategoryId}", productId, categoryId);
			storage.DeleteProduct(categoryId, productId);
			notifier.SendNotification($"В каталоге из категории {categoryId} удален продукт: Id = {productId}.");
		}
	}
}
