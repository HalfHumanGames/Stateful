namespace Stateful.Utilities {

	public delegate void ValueChangedHandler<T>(Observable<T> observable, T previousValue, T newValue);
	public delegate void ChangedHandler(INotifyChanged observable);

	public interface INotifyChanged {
		event ChangedHandler Changed;
	}

	public class Observable<T> : INotifyChanged {

		public event ChangedHandler Changed;
		public event ValueChangedHandler<T> ValueChanged;

		private T value;
		public T Value {
			get => value;
			set {
				if (this.value.Equals(value)) {
					return;
				}
				T temp = this.value;
				this.value = value;
				Changed?.Invoke((INotifyChanged) this);
				ValueChanged?.Invoke(this, temp, this.value);
			}
		}

		public Observable() { }
		public Observable(T value) {
			this.value = value;
		}
	}
}
