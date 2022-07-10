using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ProductCatalog.Models
{
	public class CatalogStorage : ICatalogStorage
	{
		private class CategoryStorage : Category
		{
			private ConcurrentDictionary<int, Product> Products { get; set; }
			private readonly ILogger<CatalogStorage> logger;

			private class CategoryEnumerator : IEnumerable, IEnumerable<Product>
			{
				private readonly CategoryStorage category;

				public CategoryEnumerator(CategoryStorage category)
				{
					this.category = category;
					category.logger.LogDebug("(CategoryStorage {CategoryId}) создан CategoryEnumerator", category.Id);
				}

				IEnumerator IEnumerable.GetEnumerator()
				{
					foreach (var p in category.Products) yield return p.Value;
				}

				IEnumerator<Product> IEnumerable<Product>.GetEnumerator()
				{
					foreach (var p in category.Products) yield return p.Value;
				}
			}

			public CategoryStorage(int Id, ILogger<CatalogStorage> logger) : base(Id)
			{
				Products = new ConcurrentDictionary<int, Product>();
				this.logger = logger;
				logger.LogInformation("(CategoryStorage {CategoryId}) создано хранилище", Id);
			}

			public int Count => Products.Count;

			public Product GetProduct(int productId)
			{
				try
				{
					return Products[productId];
				} catch (KeyNotFoundException)
				{
					logger.LogDebug("(CategoryStorage {CategoryId}) продукт с кодом {ProductId} не найден", Id, productId);
					throw new CatalogException($"Продукт с кодом {productId} в категории {Id} не найден");
				}
			}

			public IEnumerable<Product> GetAllProducts() => new CategoryEnumerator(this);

			public void AddProduct(Product newData)
			{
				logger.LogTrace("(CategoryStorage {CategoryId}) добавление продукта {@newData}", Id, newData);
				if (Products.TryAdd(newData.Id, newData)) return;
				logger.LogDebug("(CategoryStorage {CategoryId}) продукт с кодом {ProductId} уже существует", Id, newData.Id);
				throw new CatalogException($"Продукт c кодом {newData.Id} в категории {Id} уже существует");
			}

			public void UpdateProduct(Product newData)
			{
				logger.LogTrace("(CategoryStorage {CategoryId}) изменение продукта {@newData}", Id, newData);
				while (true)
				{
					try
					{
						if (Products.TryUpdate(newData.Id, newData, Products[newData.Id])) return;
					} catch (KeyNotFoundException)
					{
						logger.LogDebug("(CategoryStorage {CategoryId}) продукт с кодом {ProductId} не найден", Id, newData.Id);
						throw new CatalogException($"Продукт с кодом {newData.Id} в категории {Id} не найден");
					}
				}
			}

			public void DeleteProduct(int productId)
			{
				logger.LogTrace("(CategoryStorage {CategoryId}) удаление продукта {ProductId}", Id, productId);
				if (Products.TryRemove(productId, out _)) return;
				logger.LogDebug("(CategoryStorage {CategoryId}) продукт с кодом {ProductId} не найден", Id, productId);
				throw new CatalogException($"Продукт с кодом {productId} в категории {Id} не найден");
			}
		}

		private ConcurrentDictionary<int, CategoryStorage> Categories { get; set; }
		private readonly ILogger<CatalogStorage> logger;

		private class CatalogEnumerator : IEnumerable, IEnumerable<Category>
		{
			private readonly CatalogStorage catalog;

			public CatalogEnumerator(CatalogStorage catalog)
			{
				this.catalog = catalog;
				catalog.logger.LogDebug("Cоздан CatalogEnumerator");
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				foreach (var c in catalog.Categories) yield return c.Value;
			}

			IEnumerator<Category> IEnumerable<Category>.GetEnumerator()
			{
				foreach (var c in catalog.Categories) yield return c.Value;
			}
		}

		public CatalogStorage(ILogger<CatalogStorage> logger)
		{
			this.logger = logger;
			Categories = new ConcurrentDictionary<int, CategoryStorage>();
			AddTestData();
			logger.LogInformation("Основное хранилище создано");
		}

		public int CountCategories() => Categories.Count;

		public bool HasAnyCategories() => CountCategories() > 0;

		private CategoryStorage GetCategoryStorage(int categoryId)
		{
			try
			{
				return Categories[categoryId];
			} catch (KeyNotFoundException)
			{
				logger.LogWarning("Категория с кодом {CategoryId} не найдена", categoryId);
				throw new CatalogException($"Категория с кодом {categoryId} не найдена");
			}
		}

		public Category GetCategory(int categoryId) => GetCategoryStorage(categoryId);

		public IEnumerable<Category> GetAllCategories() => new CatalogEnumerator(this);

		public void AddCategory(Category newData)
		{
			logger.LogTrace("Добавление категории {@newData}", newData);
			CategoryStorage c = new CategoryStorage(newData.Id, logger) { Name = newData.Name };
			if (Categories.TryAdd(c.Id, c)) return;
			logger.LogWarning("Категория с кодом {CategoryId} уже существует", c.Id);
			throw new CatalogException($"Категория с кодом {c.Id} уже существует");
		}

		public void UpdateCategory(Category newData)
		{
			logger.LogTrace("Изменение категории {@newData}", newData);
			try
			{
				Categories[newData.Id].Name = newData.Name;
			} catch (KeyNotFoundException)
			{
				logger.LogWarning("Категория с кодом {CategoryId} не найдена", newData.Id);
				throw new CatalogException($"Категория с кодом {newData.Id} не найдена");
			}
		}

		public void DeleteCategory(int categoryId)
		{
			logger.LogTrace("Удаление категории {CategoryId}", categoryId);
			if (Categories.TryRemove(categoryId, out _)) return;
			logger.LogWarning("Категория с кодом {CategoryId} не найдена", categoryId);
			throw new CatalogException($"Категория с кодом {categoryId} не найдена");
		}

		public int CountProducts(int categoryId) => GetCategoryStorage(categoryId).Count;

		public bool HasAnyProducts(int categoryId) => CountProducts(categoryId) > 0;

		public Product GetProduct(int categoryId, int productId) => GetCategoryStorage(categoryId).GetProduct(productId);

		public IEnumerable<Product> GetAllProducts(int categoryId) => GetCategoryStorage(categoryId).GetAllProducts();

		public void AddProduct(int categoryId, Product newData)
		{
			logger.LogTrace("Добавление продукта {@newData} в категорию {CategoryId}", newData, categoryId);
			GetCategoryStorage(categoryId).AddProduct(newData);
		}

		public void UpdateProduct(int categoryId, Product newData)
		{
			logger.LogTrace("Изменение продукта {@newData} в категории {CategoryId}", newData, categoryId);
			GetCategoryStorage(categoryId).UpdateProduct(newData);
		}

		public void DeleteProduct(int categoryId, int productId)
		{
			logger.LogTrace("Удаление продукта {ProductId} из категории {CategoryId}", productId, categoryId);
			GetCategoryStorage(categoryId).DeleteProduct(productId);
		}

		private void AddTestData()
		{
			CategoryStorage c;
			c = new CategoryStorage(1, logger) { Name = "Продукты питания" };
			c.AddProduct(new Product(101) { Name = "Хлеб", ImgUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/a/ad/%D0%91%D0%B0%D1%82%D0%BE%D0%BD_%D0%A1%D0%BB%D0%BE%D0%B1%D0%BE%D0%B6%D0%B0%D0%BD%D1%81%D0%BA%D0%B8%D0%B9_%D0%A5%D0%B0%D1%80%D1%8C%D0%BA%D0%BE%D0%B2.JPG/800px-%D0%91%D0%B0%D1%82%D0%BE%D0%BD_%D0%A1%D0%BB%D0%BE%D0%B1%D0%BE%D0%B6%D0%B0%D0%BD%D1%81%D0%BA%D0%B8%D0%B9_%D0%A5%D0%B0%D1%80%D1%8C%D0%BA%D0%BE%D0%B2.JPG", Price = 40 });
			c.AddProduct(new Product(102) { Name = "Молоко", ImgUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/0/0e/Milk_glass.jpg/532px-Milk_glass.jpg", Price = 100 });
			Categories.TryAdd(c.Id, c);
			c = new CategoryStorage(2, logger) { Name = "Сотовые телефоны" };
			c.AddProduct(new Product(201) { Name = "Nokia 3310", ImgUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/7/78/Nokia_3310_Blue_R7309170_%28retouch%29.png/263px-Nokia_3310_Blue_R7309170_%28retouch%29.png", Price = 5000 });
			c.AddProduct(new Product(202) { Name = "iPhone 13", ImgUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/0/0c/IPhone_13_vector.svg/298px-IPhone_13_vector.svg.png", Price = 70000 });
			Categories.TryAdd(c.Id, c);
		}
	}
}