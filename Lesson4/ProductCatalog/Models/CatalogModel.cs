using ProductCatalog.Services;
using System.Collections.Generic;

namespace ProductCatalog.Models
{
	public class CatalogModel : ICatalogModel
	{
		private readonly ICatalogStorage storage;
		private readonly IMailNotifier notifier;

		public CatalogModel(ICatalogStorage storage, IMailNotifier notifier)
		{
			this.storage = storage;
			this.notifier = notifier;
		}

		public int CountCategories() => storage.CountCategories();

		public bool HasAnyCategories() => storage.HasAnyCategories();

		public Category GetCategory(int categoryId) => storage.GetCategory(categoryId);

		public IEnumerable<Category> GetAllCategories() => storage.GetAllCategories();

		public string AddCategory(Category newData)
		{
			var result = storage.AddCategory(newData);
			if (result != null) return result;
			notifier.SendNotification($"В каталоге создана новая категория: Id = {newData.Id}, Name = {newData.Name}.");
			return null;
		}

		public string UpdateCategory(Category newData)
		{
			var result = storage.UpdateCategory(newData);
			if (result != null) return result;
			notifier.SendNotification($"В каталоге изменена категория: Id = {newData.Id}, Name = {newData.Name}.");
			return null;
		}

		public void DeleteCategory(int categoryId)
		{
			storage.DeleteCategory(categoryId);
			notifier.SendNotification($"В каталоге удалена категория: Id = {categoryId}.");
		}

		public int CountProducts(int categoryId) => storage.CountProducts(categoryId);

		public bool HasAnyProducts(int categoryId) => storage.HasAnyProducts(categoryId);

		public Product GetProduct(int categoryId, int productId) => storage.GetProduct(categoryId, productId);

		public IEnumerable<Product> GetAllProducts(int categoryId) => storage.GetAllProducts(categoryId);

		public string AddProduct(int categoryId, Product newData)
		{
			var result = storage.AddProduct(categoryId, newData);
			if (result != null) return result;
			notifier.SendNotification($"В каталоге в категории {categoryId} создан новый продукт: Id = {newData.Id}, Name = {newData.Name}.");
			return null;
		}

		public string UpdateProduct(int categoryId, Product newData)
		{
			var result = storage.UpdateProduct(categoryId, newData);
			if (result != null) return result;
			notifier.SendNotification($"В каталоге в категории {categoryId} изменен продукт: Id = {newData.Id}, Name = {newData.Name}.");
			return null;
		}

		public void DeleteProduct(int categoryId, int productId)
		{
			storage.DeleteProduct(categoryId, productId);
			notifier.SendNotification($"В каталоге в категории {categoryId} удален продукт: Id = {productId}.");
		}
	}
}
