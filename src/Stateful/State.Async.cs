#if TASKS
#if NETSTANDARD1_0 || NET45
using Stateful.Utilities;
#endif
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stateful {

	public partial class State<TStateId, TParamId, TMessageId> {

		private StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler onEnterAsync;
		private StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler onExitAsync;
		private StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler onPauseAsync;
		private StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler onResumeAsync;
		private Dictionary<TMessageId, StateMachine<TStateId, TParamId, TMessageId>.OnMessageAsyncHandler> onMessagesAsync =
			new Dictionary<TMessageId, StateMachine<TStateId, TParamId, TMessageId>.OnMessageAsyncHandler>();
		private Dictionary<TMessageId, StateMachine<TStateId, TParamId, TMessageId>.OnMessageAsyncHandler<object>> onMessagesWithReturnValueAsync =
			new Dictionary<TMessageId, StateMachine<TStateId, TParamId, TMessageId>.OnMessageAsyncHandler<object>>();

		#region Internal transition handler wrappers used by state machine

		internal virtual async Task EnterAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) {
			await (onEnterAsync == null ? OnEnterAsync(stateMachine) : Task.WhenAll(onEnterAsync(stateMachine, this), OnEnterAsync(stateMachine)));
		}

		internal virtual async Task ExitAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) {
			await (onExitAsync == null ? OnExitAsync(stateMachine) : Task.WhenAll(onExitAsync(stateMachine, this), OnExitAsync(stateMachine)));
		}

		internal virtual async Task PauseAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) {
			await (onPauseAsync == null ? OnPauseAsync(stateMachine) : Task.WhenAll(onPauseAsync(stateMachine, this), OnPauseAsync(stateMachine)));
		}

		internal virtual async Task ResumeAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) {
			await (onResumeAsync == null ? OnResumeAsync(stateMachine) : Task.WhenAll(onResumeAsync(stateMachine, this), OnResumeAsync(stateMachine)));
		}

		internal virtual async Task SendMessageAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine, TMessageId message, object arg) {
			if (onMessagesAsync.ContainsKey(message)) {
				throw new ArgumentException($"No message with the id {message} found.");
			}
			await onMessagesAsync[message](stateMachine, this, arg);
		}

		internal virtual async Task<T> SendMessageAsync<T>(StateMachine<TStateId, TParamId, TMessageId> stateMachine, TMessageId message, object arg) {
			if (!onMessagesWithReturnValueAsync.ContainsKey(message)) {
				throw new ArgumentException($"No message with the id {message} found.");
			}
			object retval = await onMessagesWithReturnValueAsync[message](stateMachine, this, arg);
			if (!(retval is T)) {
				throw new ArgumentException($"Return value is not of type {typeof(T)}");
			}
			return (T) retval;
		}

		#endregion

		#region Internal on transition or message setters used by state machine builder

		internal State<TStateId, TParamId, TMessageId> OnEnterAsync(StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler handler) {
			onEnterAsync = handler;
			return this;
		}

		internal State<TStateId, TParamId, TMessageId> OnExitAsync(StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler handler) {
			onExitAsync = handler;
			return this;
		}

		internal State<TStateId, TParamId, TMessageId> OnPauseAsync(StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler handler) {
			onPauseAsync = handler;
			return this;
		}

		internal State<TStateId, TParamId, TMessageId> OnResumeAsync(StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler handler) {
			onResumeAsync = handler;
			return this;
		}

		internal State<TStateId, TParamId, TMessageId> OnAsync(TMessageId message, StateMachine<TStateId, TParamId, TMessageId>.OnMessageAsyncHandler handler) {
			onMessagesAsync[message] = handler;
			return this;
		}

		internal State<TStateId, TParamId, TMessageId> OnAsync<T>(TMessageId message, StateMachine<TStateId, TParamId, TMessageId>.OnMessageAsyncHandler<T> handler) {
			onMessagesWithReturnValueAsync[message] = async (machine, state, data) => await handler(machine, state, data);
			return this;
		}

		#endregion

		#region Protected on transition methods available for override

		protected virtual async Task OnEnterAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) {
#if NETSTANDARD1_0 || NET45
			await TaskUtility.CompletedTask;
#else
			await Task.CompletedTask;
#endif
		}

		protected virtual async Task OnExitAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) {
#if NETSTANDARD1_0 || NET45
			await TaskUtility.CompletedTask;
#else
			await Task.CompletedTask;
#endif
		}

		protected virtual async Task OnPauseAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) {
#if NETSTANDARD1_0 || NET45
			await TaskUtility.CompletedTask;
#else
			await Task.CompletedTask;
#endif
		}

		protected virtual async Task OnResumeAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) {
#if NETSTANDARD1_0 || NET45
			await TaskUtility.CompletedTask;
#else
			await Task.CompletedTask;
#endif
		}

		#endregion
	}
}
#endif
