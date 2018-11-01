using System;

namespace StateMachineNet.Utilities {

	[Serializable] 
	public class Ref<T> where T : struct {

		public T Value { get; set; }
		public Ref(T value) => Value = value;
	}
}