using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProductCatalog.Models
{
	public class Product
	{
		public readonly int Id;
		public string Name { get; set; }
		public string ImgUrl { get; set; }
		public decimal Price { get; set; }

		public Product(int Id) { this.Id = Id; }
	}

	public class ProductAddingModel
	{
		public int Id { get; set; }

		[Required]
		public string Name { get; set; }
		public string ImgUrl { get; set; }
		public decimal Price { get; set; }
	}

	public class Category
	{
		public readonly int Id;
		public string Name { get; set; }

		public Category(int Id) { this.Id = Id; }
	}

	public class CategoryAddingModel
	{
		public int Id { get; set; }

		[Required]
		public string Name { get; set; }
	}
}
