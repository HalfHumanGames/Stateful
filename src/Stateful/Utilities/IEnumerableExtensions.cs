using System;
using System.Collections.Generic;

namespace Stateful.Utilities {

	public static class IEnumerableExtensions {

		public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action) {
			foreach (T item in collection) {
				action(item);
			}
		}
	}
}
