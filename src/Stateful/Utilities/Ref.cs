using System;

namespace Stateful.Utilities {

	public class Ref<T> where T : struct {

		public T Value { get; set; }
		public Ref(T value) => Value = value;
	}
}
