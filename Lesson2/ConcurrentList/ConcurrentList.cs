using System;
using System.Collections;
using System.Collections.Generic;

namespace ConcurrentList
{
	public class ConcurrentList<T> : ICollection<T>
	{
		// Состояние элемента: существующий, помеченный как удаленный, "совсем удаленный"
		private enum ElementState { Valid, Deleted, Purged }

		// Элемент списка
		private class Element
		{
			public readonly T Data;              // Полезные данные
			public volatile Element Next;        // Указатель на следующий элемент списка
			public volatile ElementState State;  // Состояние элемента

			public Element(T Data) { this.Data = Data; Next = null; State = ElementState.Valid; }
		}

		private volatile Element First;          // Указатель на первый элемент списка
		private readonly Comparison<T> SortRule; // Правило сортировки для отсортированного списка
		private object lockObject;               // Объект для синхронизации

		public bool IsReadOnly => false;

		public ConcurrentList(Comparison<T> SortRule = null)
		{
			First = null; lockObject = new object(); this.SortRule = SortRule;
		}

		// ------------------------------------------------------------ Перечисление, подсчет и поиск элементов
		private IEnumerator<T> GetEnumerator()
		{
			for (Element Curr = First; Curr != null; Curr = Curr.Next)
				if (Curr.State == ElementState.Valid) yield return Curr.Data;
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public int CountConditional(Predicate<T> Filter = null)
		{
			int n = 0;
			foreach (T x in (IEnumerable)this) if ((Filter == null) || Filter(x)) n++;
			return n;
		}

		public int Count => CountConditional();

		public bool Contains(T Data)
		{
			foreach (T x in (IEnumerable)this) if (x.Equals(Data)) return true;
			return false;
		}

		// ------------------------------------------------------------ Преобразование в другие структуры

		// Копирует элементы в указанный массив, стандартный метод ICollection
		public void CopyTo(T[] ToArray, int FromIndex)
		{
			if (ToArray == null) throw new ArgumentNullException();
			if (FromIndex < 0) throw new ArgumentOutOfRangeException();
			foreach (T e in (IEnumerable)this)
			{
				if (FromIndex == ToArray.Length) throw new ArgumentException();
				ToArray[FromIndex++] = e;
			}
		}

		// Создает массив элементов - выборку из списка по данному предикату
		public List<T> Snapshot(Predicate<T> Filter = null)
		{
			int n = CountConditional(Filter);
			if (n == 0) return null;
			var Result = new List<T>(n);
			foreach (T e in (IEnumerable)this) if ((Filter == null) || Filter(e)) Result.Add(e);
			return Result;
		}

		// ------------------------------------------------------------ Добавление элементов
		public void Add(T Data)
		{
			var Elem = new Element(Data);
			if (SortRule == null)
			{
				lock (lockObject)
				{
					Elem.Next = First; First = Elem;
				}
				return;
			}
			Element Prev;
			while (true)
			{
				// Ищем место для вставки: либо после элемента Prev, либо в начало, если Prev = null
				if ((First == null) || (SortRule(Data, First.Data) < 0))
				{
					Prev = null;
				} else
				{
					for (Prev = First; Prev.Next != null; Prev = Prev.Next) if (SortRule(Data, Prev.Next.Data) < 0) break;
				}
				lock (lockObject)
				{
					// Пока мы пытались заблокировать коллекцию, она могла измениться
					// Если были вставлены новые элементы - возможно, надо пропустить еще несколько
					// Но идея в том, что большую часть элементов мы просмотрим и примерное место вставки найдем до блокировки
					if (Prev == null)
					{
						// Мы планировали вставлять в начало
						// Если так и надо - вставляем и выходим
						if ((First == null) || (SortRule(Data, First.Data) < 0))
						{
							Elem.Next = First; First = Elem; return;
						}
						// Иначе - вставлять надо после первого, поищем место
						Prev = First;
					}
					// Prev в любом случае меньше Elem, и вставлять надо после него
					// Но, возможно, надо пропустить еще несколько
					for (; Prev.Next != null; Prev = Prev.Next) if (SortRule(Data, Prev.Next.Data) < 0) break;
					// Если за это время не случился Purge и Prev валиден - просто вставляем после него
					if (Prev.State != ElementState.Purged)
					{
						Elem.Next = Prev.Next; Prev.Next = Elem; return;
					}
					// Нам не повезло - случился Purge и элемент, после которого мы хотели вставлять, уничтожен
					// Тогда начинаем всю попытку вставки заново; но идея в том, что Purge - дело редкое, и такие
					// коллизии будут редкими
				}
			}
		}

		// ------------------------------------------------------------ Удаление элементов

		// Удаляет из списка элементы по указанному условию, все (по умолчанию) или только первый
		public int Delete(Predicate<T> Filter, bool OnlyFirst = false)
		{
			int num = 0;

			for (Element Curr = First; Curr != null; Curr = Curr.Next)
			{
				if ((Curr.State != ElementState.Valid) || (!Filter(Curr.Data))) continue;
				lock (lockObject)
				{
					if (Curr.State != ElementState.Valid) continue;
					Curr.State = ElementState.Deleted;
				}
				num++; if (OnlyFirst) return 1;
			}
			return num;
		}

		// Восстанавливает в списке элементы по указанному условию, все (по умолчанию) или только первый
		public int Undelete(Predicate<T> Filter, bool OnlyFirst = false)
		{
			int num = 0;

			for (Element Curr = First; Curr != null; Curr = Curr.Next)
			{
				if ((Curr.State != ElementState.Deleted) || (!Filter(Curr.Data))) continue;
				lock (lockObject)
				{
					if (Curr.State != ElementState.Deleted) continue;
					Curr.State = ElementState.Valid;
				}
				num++; if (OnlyFirst) return 1;
			}
			return num;
		}

		public bool Remove(T Data) => (Delete(e => (e.Equals(Data)), true) == 1);

		// Фактически удаляет из списка все удаленные элементы
		public void Purge()
		{
			lock (lockObject)
			{
				while ((First != null) && (First.State == ElementState.Deleted))
				{
					First.State = ElementState.Purged;
					First = First.Next;
				}
				for (var Curr = First; Curr != null; Curr = Curr.Next)
				{
					while ((Curr.Next != null) && (Curr.Next.State == ElementState.Deleted))
					{
						Curr.Next.State = ElementState.Purged;
						Curr.Next = Curr.Next.Next;
					}
				}
			}
		}

		// Удаляет из списка все элементы
		public void Clear()
		{
			lock (lockObject)
			{
				while (First != null)
				{
					First.State = ElementState.Purged;
					First = First.Next;
				}
			}
		}
	}
}
