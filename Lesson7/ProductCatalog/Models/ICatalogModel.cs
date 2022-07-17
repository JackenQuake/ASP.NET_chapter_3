using System.Collections.Generic;
using System.Threading;

namespace ProductCatalog.Models
{
	public interface ICatalogModel
	{
		public int CountCategories(CancellationToken token = default);

		public bool HasAnyCategories(CancellationToken token = default);

		public Category GetCategory(int categoryId, CancellationToken token = default);

		public IEnumerable<Category> GetAllCategories(CancellationToken token = default);

		public void AddCategory(Category newData, CancellationToken token = default);

		public void UpdateCategory(Category newData, CancellationToken token = default);

		public void DeleteCategory(int categoryId, CancellationToken token = default);

		public int CountProducts(int categoryId, CancellationToken token = default);

		public bool HasAnyProducts(int categoryId, CancellationToken token = default);

		public Product GetProduct(int categoryId, int productId, CancellationToken token = default);

		public IEnumerable<Product> GetAllProducts(int categoryId, CancellationToken token = default);

		public void AddProduct(int categoryId, Product newData, CancellationToken token = default);

		public void UpdateProduct(int categoryId, Product newData, CancellationToken token = default);

		public void DeleteProduct(int categoryId, int productId, CancellationToken token = default);
	}
}
