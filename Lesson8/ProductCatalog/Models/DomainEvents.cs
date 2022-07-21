using DomainEventServices;
using System;

namespace ProductCatalog.Models
{
	class CatalogChangeEvent : DomainEvent
	{
		public readonly string Message;

		public CatalogChangeEvent(string Message) { this.Message = Message; }
	}

	class CatalogAddCategoryEvent : CatalogChangeEvent
	{
		public readonly Category newData;

		public CatalogAddCategoryEvent(Category newData) :
			base($"В каталоге добавлена новая категория: Id = {newData.Id}, Name = {newData.Name}.")
		{
			this.newData = newData;
		}
	}

	class CatalogUpdateCategoryEvent : CatalogChangeEvent
	{
		public readonly Category newData;

		public CatalogUpdateCategoryEvent(Category newData) :
			base($"В каталоге изменена категория: Id = {newData.Id}, Name = {newData.Name}.")
		{
			this.newData = newData;
		}
	}

	class CatalogDeleteCategoryEvent : CatalogChangeEvent
	{
		public readonly int categoryId;

		public CatalogDeleteCategoryEvent(int categoryId) :
			base($"В каталоге удалена категория: Id = {categoryId}.")
		{
			this.categoryId = categoryId;
		}
	}

	class CatalogAddProductEvent : CatalogChangeEvent
	{
		public readonly int categoryId;
		public readonly Product newData;

		public CatalogAddProductEvent(int categoryId, Product newData) :
			base($"В каталоге в категорию {categoryId} добавлен новый продукт: Id = {newData.Id}, Name = {newData.Name}.")
		{
			this.categoryId = categoryId;
			this.newData = newData;
		}
	}

	class CatalogUpdateProductEvent : CatalogChangeEvent
	{
		public readonly int categoryId;
		public readonly Product newData;

		public CatalogUpdateProductEvent(int categoryId, Product newData) :
			base($"В каталоге в категории {categoryId} изменен продукт: Id = {newData.Id}, Name = {newData.Name}.")
		{
			this.categoryId = categoryId;
			this.newData = newData;
		}
	}

	class CatalogDeleteProductEvent : CatalogChangeEvent
	{
		public readonly int categoryId;
		public readonly int productId;

		public CatalogDeleteProductEvent(int categoryId, int productId) :
			base($"В каталоге из категории {categoryId} удален продукт: Id = {productId}.")
		{
			this.categoryId = categoryId;
			this.productId = productId;
		}
	}

	class CatalogErrorEvent : DomainEvent
	{
		public readonly string Message;
		public readonly Exception Error;

		public CatalogErrorEvent(string Message, Exception Error)
		{
			this.Message = Message;
			this.Error = Error;
		}
	}
}
