using System.Collections;
using System.Collections.Generic;

namespace SiteMetrics
{
	public struct MetricsData
	{
		public readonly string Path;
		public readonly int Counter;

		public MetricsData(string Path, int Counter)
		{
			this.Path = Path;
			this.Counter = Counter;
		}
	}

	public interface IMetricsStorage : IEnumerable, IEnumerable<MetricsData>
	{
		public int Increment(string Path);
	}
}
