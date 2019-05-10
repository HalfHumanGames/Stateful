#if NETSTANDARD1_0
using System;
using System.Collections.Generic;
#endif 

namespace Stateful.Utilities {

	public static class IEnumerableExtensions {

		#if NETSTANDARD1_0
		public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action) {
			foreach (T item in collection) {
				action(item);
			}
		}
		#endif
	}
}
