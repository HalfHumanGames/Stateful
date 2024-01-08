using System;
using System.Collections.Generic;

namespace Stateful {

	#region Subclasses

	public class State : State<string, string, string> { }
	public class State<TStateId> : State<TStateId, string, string> { }
	public class State<TStateId, TParamId> : State<TStateId, TParamId, string> { }

	#endregion

	public partial class State<TStateId, TParamId, TMessageId> {

		private StateMachine<TStateId, TParamId, TMessageId>.OnTransitionHandler onEnter;
		private StateMachine<TStateId, TParamId, TMessageId>.OnTransitionHandler onExit;
		private StateMachine<TStateId, TParamId, TMessageId>.OnTransitionHandler onPause;
		private StateMachine<TStateId, TParamId, TMessageId>.OnTransitionHandler onResume;

		private Dictionary<TMessageId, StateMachine<TStateId, TParamId, TMessageId>.OnMessageHandler> onMessages =
			new Dictionary<TMessageId, StateMachine<TStateId, TParamId, TMessageId>.OnMessageHandler>();
		private Dictionary<TMessageId, StateMachine<TStateId, TParamId, TMessageId>.OnMessageHandler<object>> onMessagesWithReturnValue =
			new Dictionary<TMessageId, StateMachine<TStateId, TParamId, TMessageId>.OnMessageHandler<object>>();

		public State() { }

		#region Internal transition handler wrappers used by state machine

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
				throw new ArgumentException($"No message with the id {message} found.");
			}
			onMessages[message].Invoke(stateMachine, this, arg);
		}

		internal virtual T SendMessage<T>(StateMachine<TStateId, TParamId, TMessageId> stateMachine, TMessageId message, object arg) {
			if (!onMessagesWithReturnValue.ContainsKey(message)) {
				throw new ArgumentException($"No message with the id {message} found.");
			}
			object retval = onMessagesWithReturnValue[message](stateMachine, this, arg);
			if (!(retval is T)) {
				throw new ArgumentException($"Return value is not of type {typeof(T)}");
			}
			return (T) retval;
		}

		#endregion

		#region Internal on transition or message setters used by state machine builder

		internal State<TStateId, TParamId, TMessageId> OnEnter(StateMachine<TStateId, TParamId, TMessageId>.OnTransitionHandler handler) {
			onEnter = handler;
			return this;
		}

		internal State<TStateId, TParamId, TMessageId> OnExit(StateMachine<TStateId, TParamId, TMessageId>.OnTransitionHandler handler) {
			onExit = handler;
			return this;
		}

		internal State<TStateId, TParamId, TMessageId> OnPause(StateMachine<TStateId, TParamId, TMessageId>.OnTransitionHandler handler) {
			onPause = handler;
			return this;
		}

		internal State<TStateId, TParamId, TMessageId> OnResume(StateMachine<TStateId, TParamId, TMessageId>.OnTransitionHandler handler) {
			onResume = handler;
			return this;
		}

		internal State<TStateId, TParamId, TMessageId> On(TMessageId message, StateMachine<TStateId, TParamId, TMessageId>.OnMessageHandler handler) {
			onMessages[message] = handler;
			return this;
		}

		internal State<TStateId, TParamId, TMessageId> On<T>(TMessageId message, StateMachine<TStateId, TParamId, TMessageId>.OnMessageHandler<T> handler) {
			onMessagesWithReturnValue[message] = (machine, state, data) => handler(machine, state, data);
			return this;
		}

		#endregion

		#region Protected on transition methods available for override

		protected virtual void OnEnter(StateMachine<TStateId, TParamId, TMessageId> stateMachine) { }
		protected virtual void OnExit(StateMachine<TStateId, TParamId, TMessageId> stateMachine) { }
		protected virtual void OnPause(StateMachine<TStateId, TParamId, TMessageId> stateMachine) { }
		protected virtual void OnResume(StateMachine<TStateId, TParamId, TMessageId> stateMachine) { }

		#endregion

	}
}
