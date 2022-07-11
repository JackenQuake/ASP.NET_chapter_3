using System;

namespace ProductCatalog.Models
{
	public class CatalogException : ArgumentException
	{
		public CatalogException(string message) : base(message) {}
	}
}
