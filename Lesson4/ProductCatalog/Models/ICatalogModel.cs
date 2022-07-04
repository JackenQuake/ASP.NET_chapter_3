using System.Collections.Generic;

namespace ProductCatalog.Models
{
	public interface ICatalogModel
	{
		public int CountCategories();

		public bool HasAnyCategories();

		public Category GetCategory(int categoryId);

		public IEnumerable<Category> GetAllCategories();

		public string AddCategory(Category newData);

		public string UpdateCategory(Category newData);

		public void DeleteCategory(int categoryId);

		public int CountProducts(int categoryId);

		public bool HasAnyProducts(int categoryId);

		public Product GetProduct(int categoryId, int productId);

		public IEnumerable<Product> GetAllProducts(int categoryId);

		public string AddProduct(int categoryId, Product newData);

		public string UpdateProduct(int categoryId, Product newData);

		public void DeleteProduct(int categoryId, int productId);
	}
}
