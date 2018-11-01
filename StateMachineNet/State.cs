using System;
using System.Collections.Generic;

namespace StateMachineNet {

	#region Subclasses

	[Serializable] public class State : State<string, string, string> { }
	[Serializable] public class State<TStateId> : State<TStateId, string, string> { }
	[Serializable] public class State<TStateId, TParamId> : State<TStateId, TParamId, string> { }

	#endregion

	[Serializable]
	public partial class State<TStateId, TParamId, TMessageId> {

		protected StateMachine<TStateId, TParamId, TMessageId>.OnTransitionHandler onEnter;
		protected StateMachine<TStateId, TParamId, TMessageId>.OnTransitionHandler onExit;
		protected StateMachine<TStateId, TParamId, TMessageId>.OnTransitionHandler onPause;
		protected StateMachine<TStateId, TParamId, TMessageId>.OnTransitionHandler onResume;
		protected Dictionary<TMessageId, StateMachine<TStateId, TParamId, TMessageId>.OnMessageHandler> onMessages =
			  new Dictionary<TMessageId, StateMachine<TStateId, TParamId, TMessageId>.OnMessageHandler>();

		public State() { }

		#region transition action wrappers

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

		internal virtual void SendMessage(
			StateMachine<TStateId, TParamId, TMessageId> stateMachine, TMessageId message, object arg
		) {
			if (!onMessages.ContainsKey(message)) {
				return;
			}
			onMessages[message].Invoke(stateMachine, this, arg);
		}

		#endregion

		#region On transition methods

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

		#region On transition or message setters

		/// <summary>
		/// Sets the action to invoke when entering this state
		/// </summary>
		/// <param name="action">On enter action</param>
		/// <returns>Returns this state to allow method chaining</returns>
		public State<TStateId, TParamId, TMessageId> OnEnter(
			StateMachine<TStateId, TParamId, TMessageId>.OnTransitionHandler handler
		) {
			onEnter = handler;
			return this;
		}

		/// <summary>
		/// Sets the action to invoke when exiting this state
		/// </summary>
		/// <param name="action">On enter action</param>
		/// <returns>Returns this state to allow method chaining</returns>
		public State<TStateId, TParamId, TMessageId> OnExit(
			StateMachine<TStateId, TParamId, TMessageId>.OnTransitionHandler handler
		) {
			onExit = handler;
			return this;
		}

		/// <summary>
		/// Sets the action to invoke when pausing this state
		/// </summary>
		/// <param name="action">On pause action</param>
		/// <returns>Returns this state to allow method chaining</returns>
		public State<TStateId, TParamId, TMessageId> OnPause(
			StateMachine<TStateId, TParamId, TMessageId>.OnTransitionHandler handler
		) {
			onPause = handler;
			return this;
		}

		/// <summary>
		/// Sets the action to invoke when resuming this state
		/// </summary>
		/// <param name="action">On resume action</param>
		/// <returns>Returns this state to allow method chaining</returns>
		public State<TStateId, TParamId, TMessageId> OnResume(
			StateMachine<TStateId, TParamId, TMessageId>.OnTransitionHandler handler
		) {
			onResume = handler;
			return this;
		}

		public State<TStateId, TParamId, TMessageId> On(
			TMessageId message,
			StateMachine<TStateId, TParamId, TMessageId>.OnMessageHandler action
		) {
			onMessages[message] = action;
			return this;
		}

		#endregion
	}
}
