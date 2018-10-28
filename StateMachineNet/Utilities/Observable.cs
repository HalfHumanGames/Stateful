namespace StateMachineNet.Utilities {

	public class Observable<T> {

		public delegate void ValueChangedHandler(Observable<T> observable, T previousValue, T newValue);
		public event ValueChangedHandler ValueChanged;

		private T value;
		public T Value {
			get => value;
			set {
				if (this.value.Equals(value)) {
					return;
				}
				T temp = this.value;
				this.value = value;
				ValueChanged?.Invoke(this, temp, this.value);
			}
		}

		public Observable(T value) => this.value = value;
	}
}
