using System.Collections.Generic;

namespace ProductCatalog.Models
{
	public interface ICatalogStorage
	{
		public int CountCategories();

		public bool HasAnyCategories();

		public Category GetCategory(int categoryId);

		public IEnumerable<Category> GetAllCategories();

		public void AddCategory(Category newData);

		public void UpdateCategory(Category newData);

		public void DeleteCategory(int categoryId);

		public int CountProducts(int categoryId);

		public bool HasAnyProducts(int categoryId);

		public Product GetProduct(int categoryId, int productId);

		public IEnumerable<Product> GetAllProducts(int categoryId);

		public void AddProduct(int categoryId, Product newData);

		public void UpdateProduct(int categoryId, Product newData);

		public void DeleteProduct(int categoryId, int productId);
	}
}
