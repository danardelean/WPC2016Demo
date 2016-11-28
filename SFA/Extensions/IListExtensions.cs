using System.Collections.Generic;

namespace SFA.Extensions
{
	public static class IListExtensions
	{
		public static void AddRange<T>(this IList<T> collection, IEnumerable<T> items)
		{
			foreach (var i in items)
			{
				collection.Add(i);
			}
		}
	}
}
