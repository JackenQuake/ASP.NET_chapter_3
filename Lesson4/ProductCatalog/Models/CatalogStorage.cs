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

			private class CategoryEnumerator : IEnumerable, IEnumerable<Product>
			{
				private readonly CategoryStorage category;

				public CategoryEnumerator(CategoryStorage category)
				{
					this.category = category;
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

			public CategoryStorage(int Id) : base(Id)
			{
				Products = new ConcurrentDictionary<int, Product>();
			}

			public int Count => Products.Count;

			public Product GetProduct(int productId)
			{
				try
				{
					return Products[productId];
				} catch (KeyNotFoundException)
				{
					return null;
				}
			}

			public IEnumerable<Product> GetAllProducts() => new CategoryEnumerator(this);

			public string AddProduct(Product newData) => Products.TryAdd(newData.Id, newData) ? null : $"Продукт c кодом {newData.Id} уже существует";

			public string UpdateProduct(Product newData)
			{
				while (true)
				{
					try
					{
						if (Products.TryUpdate(newData.Id, newData, Products[newData.Id])) return null;
					} catch (KeyNotFoundException)
					{
						return $"Продукт c кодом {newData.Id} не найден";
					}
				}
			}

			public void DeleteProduct(int productId)
			{
				Products.TryRemove(productId, out _);
			}
		}

		private ConcurrentDictionary<int, CategoryStorage> Categories { get; set; }

		private class CatalogEnumerator : IEnumerable, IEnumerable<Category>
		{
			private readonly CatalogStorage catalog;

			public CatalogEnumerator(CatalogStorage catalog)
			{
				this.catalog = catalog;
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

		public CatalogStorage()
		{
			Categories = new ConcurrentDictionary<int, CategoryStorage>();
			AddTestData();
		}

		public int CountCategories() => Categories.Count;

		public bool HasAnyCategories() => CountCategories() > 0;

		public Category GetCategory(int categoryId)
		{
			try
			{
				return Categories[categoryId];
			} catch (KeyNotFoundException)
			{
				return null;
			}
		}

		public IEnumerable<Category> GetAllCategories() => new CatalogEnumerator(this);

		public string AddCategory(Category newData)
		{
			CategoryStorage c = new CategoryStorage(newData.Id) { Name = newData.Name };
			return Categories.TryAdd(c.Id, c) ? null : $"Категория с кодом {c.Id} уже существует";
		}

		public string UpdateCategory(Category newData)
		{
			try
			{
				Categories[newData.Id].Name = newData.Name; return null;
			} catch (KeyNotFoundException)
			{
				return $"Категория с кодом {newData.Id} не найдена";
			}
		}

		public void DeleteCategory(int categoryId)
		{
			Categories.TryRemove(categoryId, out _);
		}

		public int CountProducts(int categoryId)
		{
			try
			{
				return Categories[categoryId].Count;
			} catch (KeyNotFoundException)
			{
				return 0;
			}
		}

		public bool HasAnyProducts(int categoryId) => CountProducts(categoryId) > 0;

		public Product GetProduct(int categoryId, int productId) {
			try
			{
				return Categories[categoryId].GetProduct(productId);
			} catch (KeyNotFoundException)
			{
				return null;
			}
		}

		public IEnumerable<Product> GetAllProducts(int categoryId) {
			try
			{
				return Categories[categoryId].GetAllProducts();
			} catch (KeyNotFoundException)
			{
				return null;
			}
		}

		public string AddProduct(int categoryId, Product newData)
		{
			try
			{
				return Categories[categoryId].AddProduct(newData);
			} catch (KeyNotFoundException)
			{
				return $"Категория с кодом {categoryId} не найдена";
			}
		}

		public string UpdateProduct(int categoryId, Product newData)
		{
			try
			{
				return Categories[categoryId].UpdateProduct(newData);
			} catch (KeyNotFoundException)
			{
				return $"Категория с кодом {categoryId} не найдена";
			}
		}

		public void DeleteProduct(int categoryId, int productId)
		{
			try
			{
				Categories[categoryId].DeleteProduct(productId);
			} catch (KeyNotFoundException) { }
		}

		private void AddTestData()
		{
			CategoryStorage c;
			c = new CategoryStorage(1) { Name = "Продукты питания" };
			c.AddProduct(new Product(101) { Name = "Хлеб", ImgUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/a/ad/%D0%91%D0%B0%D1%82%D0%BE%D0%BD_%D0%A1%D0%BB%D0%BE%D0%B1%D0%BE%D0%B6%D0%B0%D0%BD%D1%81%D0%BA%D0%B8%D0%B9_%D0%A5%D0%B0%D1%80%D1%8C%D0%BA%D0%BE%D0%B2.JPG/800px-%D0%91%D0%B0%D1%82%D0%BE%D0%BD_%D0%A1%D0%BB%D0%BE%D0%B1%D0%BE%D0%B6%D0%B0%D0%BD%D1%81%D0%BA%D0%B8%D0%B9_%D0%A5%D0%B0%D1%80%D1%8C%D0%BA%D0%BE%D0%B2.JPG", Price = 40 });
			c.AddProduct(new Product(102) { Name = "Молоко", ImgUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/0/0e/Milk_glass.jpg/532px-Milk_glass.jpg", Price = 100 });
			Categories.TryAdd(c.Id, c);
			c = new CategoryStorage(2) { Name = "Сотовые телефоны" };
			c.AddProduct(new Product(201) { Name = "Nokia 3310", ImgUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/7/78/Nokia_3310_Blue_R7309170_%28retouch%29.png/263px-Nokia_3310_Blue_R7309170_%28retouch%29.png", Price = 5000 });
			c.AddProduct(new Product(202) { Name = "iPhone 13", ImgUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/0/0c/IPhone_13_vector.svg/298px-IPhone_13_vector.svg.png", Price = 70000 });
			Categories.TryAdd(c.Id, c);
		}
	}
}