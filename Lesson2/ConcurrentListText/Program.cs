using ConcurrentList;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ConcurrentListText
{
	class Program
	{
		static Random rnd = new Random();

		public class TestPoint
		{
			public int x, y;

			public TestPoint() { x = rnd.Next(100); y = rnd.Next(100); }

			public void Print() { Console.Write($"({x},{y})"); }
		}

		static ConcurrentList<TestPoint> list;

		static void PrintList()
		{
			foreach (var e in list) e.Print(); Console.WriteLine();
		}

		volatile static bool toggle = false;

		private static void ExtraThreadMethod()
		{
			TestPoint e, e1 = null, e2 = null;
			for (int i = 0; i<5; i++)
			{
				while (!toggle) ;
				e = new TestPoint();
				list.Add(e);
				if (i == 1) e1 = e;
				if (i == 3) e2 = e;
				PrintList();
				toggle = false;
			}
			while (!toggle) ;
			list.Remove(e1); list.Remove(e2);
			PrintList();
			toggle = false;
		}

		public static int CompareX(TestPoint a, TestPoint b) => (a.x - b.x);

		public static int CompareY(TestPoint a, TestPoint b) => (a.y - b.y);

		static void Main(string[] args)
		{
			list = new ConcurrentList<TestPoint>(CompareX);
			// Создадим двумя потоками отсортированную по X коллекцию из 10 точек
			Thread ExtraThread = new Thread(new ThreadStart(ExtraThreadMethod));
			ExtraThread.Start();
			TestPoint e, e1 = null, e2 = null;
			for (int i=0; i<5; i++)
			{
				while (toggle) ;
				e = new TestPoint();
				list.Add(e);
				if (i == 0) e1 = e;
				if (i == 2) e2 = e;
				PrintList();
				toggle = true;
			}
			// Удалим в двух потоках 4 точки
			while (toggle) ;
			list.Remove(e1); list.Remove(e2);
			PrintList();
			toggle = true;
			while (toggle) ;
			// Проверим Purge
			list.Purge();
			PrintList();
			Console.WriteLine($"Remaining elements: {list.Count}");
			// Сделаем snapshot, отсортируем по Y и распечатаем
			List<TestPoint> snapshot = list.Snapshot(a => (a.x < 80));
			snapshot.Sort(CompareY);
			foreach (var v in snapshot) v.Print(); Console.WriteLine();
			Console.WriteLine("Press and key to exit");
			Console.ReadKey();
		}
	}
}
