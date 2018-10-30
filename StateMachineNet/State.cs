using System;
using System.Collections.Generic;

namespace StateMachineNet {

	public class State : State<string, string, string> { }
	public class State<TStateId> : State<TStateId, string, string> { }
	public class State<TStateId, TParamId> : State<TStateId, TParamId, string> { }
	public partial class State<TStateId, TParamId, TMessageId> {

		protected Action<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>> onEnter;
		protected Action<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>> onExit;
		protected Action<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>> onPause;
		protected Action<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>> onResume;
		protected Dictionary<TMessageId, Action<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>, object>> onMessages =
			new Dictionary<TMessageId, Action<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>, object>>();
		
		public State() { }

		#region Flow control - used by state machine

		internal virtual void Enter(StateMachine<TStateId, TParamId, TMessageId> stateMachine) {
			onEnter?.Invoke(stateMachine, this);
			OnEnter(stateMachine);
		}

		internal virtual void Exit(StateMachine<TStateId, TParamId, TMessageId> stateMachine) {
			onExit?.Invoke(stateMachine, this);
			OnExit(stateMachine);
		}

		internal virtual void Pause(StateMachine<TStateId, TParamId, TMessageId> stateMachine) {
			onPause?.Invoke(stateMachine, this);
			OnPause(stateMachine);
		}

		internal virtual void Resume(StateMachine<TStateId, TParamId, TMessageId> stateMachine) {
			onResume?.Invoke(stateMachine, this);
			OnResume(stateMachine);
		}

		internal virtual void SendMessage(StateMachine<TStateId, TParamId, TMessageId> stateMachine, TMessageId message, object arg) {
			if (!onMessages.ContainsKey(message)) {
				return;
			}
			onMessages[message].Invoke(stateMachine, this, arg);
		}

		#endregion

		#region On<Transition> methods

		/// <summary>
		/// OnEnter is called when entering this state
		/// </summary>
		/// <param name="stateMachine">The state machine this state belongs to</param>
		protected virtual void OnEnter(StateMachine<TStateId, TParamId, TMessageId> stateMachine) { }

		/// <summary>
		/// OnExit is called when exiting this state
		/// </summary>
		/// <param name="stateMachine">The state machine this state belongs to</param>
		protected virtual void OnExit(StateMachine<TStateId, TParamId, TMessageId> stateMachine) { }

		/// <summary>
		/// OnPause is called when pausing this state
		/// </summary>
		/// <param name="stateMachine">The state machine this state belongs to</param>
		protected virtual void OnPause(StateMachine<TStateId, TParamId, TMessageId> stateMachine) { }

		/// <summary>
		/// OnResume is called when resuming this state
		/// </summary>
		/// <param name="stateMachine">The state machine this state belongs to</param>
		protected virtual void OnResume(StateMachine<TStateId, TParamId, TMessageId> stateMachine) { }

		#endregion

		#region On<Transition> and OnMessage setters

		/// <summary>
		/// Sets the action to invoke when entering this state
		/// </summary>
		/// <param name="action">On enter action</param>
		/// <returns>Returns this state to allow method chaining</returns>
		public State<TStateId, TParamId, TMessageId> OnEnter(
			Action<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>> action
		) {
			onEnter = action;
			return this;
		}

		/// <summary>
		/// Sets the action to invoke when exiting this state
		/// </summary>
		/// <param name="action">On enter action</param>
		/// <returns>Returns this state to allow method chaining</returns>
		public State<TStateId, TParamId, TMessageId> OnExit(
			Action<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>> action
		) {
			onExit = action;
			return this;
		}

		/// <summary>
		/// Sets the action to invoke when pausing this state
		/// </summary>
		/// <param name="action">On pause action</param>
		/// <returns>Returns this state to allow method chaining</returns>
		public State<TStateId, TParamId, TMessageId> OnPause(
			Action<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>> action
		) {
			onPause = action;
			return this;
		}

		/// <summary>
		/// Sets the action to invoke when resuming this state
		/// </summary>
		/// <param name="action">On resume action</param>
		/// <returns>Returns this state to allow method chaining</returns>
		public State<TStateId, TParamId, TMessageId> OnResume(
			Action<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>> action
		) {
			onResume = action;
			return this;
		}

		public State<TStateId, TParamId, TMessageId> On(
			TMessageId message,
			Action<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>, object> action
		) {
			onMessages[message] = action;
			return this;
		}

		#endregion
	}
}
