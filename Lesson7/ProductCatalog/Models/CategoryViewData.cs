using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace ProductCatalog.Models
{
	public class CategoryViewData : Category
	{
		public readonly ICatalogModel catalog;

		public CategoryViewData(int categoryId, ICatalogModel catalog) : base(categoryId)
		{
			this.catalog = catalog;
		}

		public int CountProducts() => catalog.CountProducts(Id);

		public bool HasAnyProducts() => catalog.HasAnyProducts(Id);

		public Product GetProduct(int productId) => catalog.GetProduct(Id, productId);

		public IEnumerable<Product> GetAllProducts() => catalog.GetAllProducts(Id);
	}
}
