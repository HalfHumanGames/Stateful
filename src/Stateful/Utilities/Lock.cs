namespace Stateful.Utilities {

	public class Lock {

		public bool IsLocked => locks > 0;

		private int locks;

		public void AddLock() {
			locks++;
		}

		public void RemoveLock() {
			if (locks > 0) {
				locks--;
			}
		}
	}
}
