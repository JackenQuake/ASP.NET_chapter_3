using Microsoft.Extensions.Logging;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace SiteMetrics
{
	public class MetricsStorage : IMetricsStorage
	{
		private class IntObject
		{
			public int Value;
		}

		private readonly ILogger<MetricsStorage> logger;
		private readonly ConcurrentDictionary<string, IntObject> Data;

		public MetricsStorage(ILogger<MetricsStorage> logger)
		{
			this.logger = logger;
			Data = new();
			logger.LogInformation("MetricsStorage создан.");
		}

		public int Increment(string Path)
		{
			logger.LogDebug("MetricsStorage: подсчет {Path}.", Path);
			while (true)
			{
				if (Data.TryGetValue(Path, out IntObject Counter))
				{
					Interlocked.Increment(ref Counter.Value);
					return Counter.Value;
				}
				Data.TryAdd(Path, new IntObject() { Value = 0 });
			}
		}

		private IEnumerator<MetricsData> GetEnumerator()
		{
			foreach (var p in Data)
				yield return new MetricsData(p.Key, p.Value.Value);
		}

		IEnumerator<MetricsData> IEnumerable<MetricsData>.GetEnumerator() => GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
