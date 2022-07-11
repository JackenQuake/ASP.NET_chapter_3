using System;
using System.IO;
using System.Threading.Tasks;

namespace MultiFileRead
{
	class Program
	{
		static async Task<string []> ReadManyFiles(params string[] files)
		{
			int numFiles = files.Length;
			if (numFiles == 0) return null;
			Task<string []> [] tasks = new Task<string []> [numFiles];
			for (int i = 0; i<numFiles; i++)
				tasks[i] = File.ReadAllLinesAsync(files[i]);
			await Task.WhenAll(tasks);
			// Теперь нужно объединить все массивы в один
			int totalSize = 0;
			for (int i = 0; i<numFiles; i++)
				totalSize += tasks[i].Result.Length;
			string[] result = new string[totalSize];
			totalSize = 0;
			for (int i = 0; i<numFiles; i++)
			{
				tasks[i].Result.CopyTo(result, totalSize);
				totalSize += tasks[i].Result.Length;
			}
			return result;
		}

		static void Main(string[] args)
		{
			Task<string[]> lines = ReadManyFiles("file1.txt", "file2.txt", "file3.txt");
			lines.Wait();
			foreach (string s in lines.Result) Console.WriteLine(s);
		}
	}
}
