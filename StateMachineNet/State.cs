using System;

namespace StateMachineNet {

	public class State : State<string, string> { }
	public class State<TStateId, TParamId> {

		protected Action<StateMachine<TStateId, TParamId>> onEnter;
		protected Action<StateMachine<TStateId, TParamId>> onExit;
		protected Action<StateMachine<TStateId, TParamId>> onPause;
		protected Action<StateMachine<TStateId, TParamId>> onResume;

		public State() { }

		#region Flow control - used by state machine

		internal virtual void Enter(StateMachine<TStateId, TParamId> stateMachine) {
			onEnter?.Invoke(stateMachine);
			OnEnter(stateMachine);
		}

		internal virtual void Exit(StateMachine<TStateId, TParamId> stateMachine) {
			onExit?.Invoke(stateMachine);
			OnEnter(stateMachine);
		}

		internal virtual void Pause(StateMachine<TStateId, TParamId> stateMachine) {
			onPause?.Invoke(stateMachine);
			OnEnter(stateMachine);
		}

		internal virtual void Resume(StateMachine<TStateId, TParamId> stateMachine) {
			onResume?.Invoke(stateMachine);
			OnEnter(stateMachine);
		}

		#endregion

		#region On<Event> methods

		/// <summary>
		/// OnEnter is called when entering this state
		/// </summary>
		/// <param name="stateMachine">The state machine this state belongs to</param>
		protected virtual void OnEnter(StateMachine<TStateId, TParamId> stateMachine) { }

		/// <summary>
		/// OnExit is called when exiting this state
		/// </summary>
		/// <param name="stateMachine">The state machine this state belongs to</param>
		protected virtual void OnExit(StateMachine<TStateId, TParamId> stateMachine) { }

		/// <summary>
		/// OnPause is called when pausing this state
		/// </summary>
		/// <param name="stateMachine">The state machine this state belongs to</param>
		protected virtual void OnPause(StateMachine<TStateId, TParamId> stateMachine) { }

		/// <summary>
		/// OnResume is called when resuming this state
		/// </summary>
		/// <param name="stateMachine">The state machine this state belongs to</param>
		protected virtual void OnResume(StateMachine<TStateId, TParamId> stateMachine) { }

		#endregion

		#region On<Event> setters

		/// <summary>
		/// Sets the action to invoke when entering this state
		/// </summary>
		/// <param name="action">On enter action</param>
		/// <returns>Returns this state to allow method chaining</returns>
		public State<TStateId, TParamId> OnEnter(Action<StateMachine<TStateId, TParamId>> action) {
			onEnter = action;
			return this;
		}

		/// <summary>
		/// Sets the action to invoke when exiting this state
		/// </summary>
		/// <param name="action">On enter action</param>
		/// <returns>Returns this state to allow method chaining</returns>
		public State<TStateId, TParamId> OnExit(Action<StateMachine<TStateId, TParamId>> action) {
			onExit = action;
			return this;
		}

		/// <summary>
		/// Sets the action to invoke when pausing this state
		/// </summary>
		/// <param name="action">On pause action</param>
		/// <returns>Returns this state to allow method chaining</returns>
		public State<TStateId, TParamId> OnPause(Action<StateMachine<TStateId, TParamId>> action) {
			onPause = action;
			return this;
		}

		/// <summary>
		/// Sets the action to invoke when resuming this state
		/// </summary>
		/// <param name="action">On resume action</param>
		/// <returns>Returns this state to allow method chaining</returns>
		public State<TStateId, TParamId> OnResume(Action<StateMachine<TStateId, TParamId>> action) {
			onResume = action;
			return this;
		}

		#endregion
	}
}
