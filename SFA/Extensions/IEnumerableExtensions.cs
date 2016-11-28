using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace SFA.Extensions
{
	public static class IEnumerableExtensions
	{
		public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> items)
		{
			var result = new ObservableCollection<T>();
			result.AddRange(items);
			return result;
		}
	}
}
