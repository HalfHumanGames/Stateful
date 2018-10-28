using System.Collections.Generic;

namespace StateMachineNet.Utilities {

	public class Lock {

		public bool IsLocked => locks.Count > 0;
		private Stack<object> locks = new Stack<object>();

		public void AddLock() => locks.Push(new object());

		public void RemoveLock() {
			if (locks.Count > 0) {
				locks.Pop();
			}
		}
	}
}
