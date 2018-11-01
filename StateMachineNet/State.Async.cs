using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateMachineNet {

	public partial class State<TStateId, TParamId, TMessageId> {

		private StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler onEnterAsync;
		private StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler onExitAsync;
		private StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler onPauseAsync;
		private StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler onResumeAsync;
		private Dictionary<TMessageId, StateMachine<TStateId, TParamId, TMessageId>.OnMessageAsyncHandler> onMessagesAsync =
			new Dictionary<TMessageId, StateMachine<TStateId, TParamId, TMessageId>.OnMessageAsyncHandler>();

		#region Internal transition handler wrappers used by state machine

		internal virtual async Task EnterAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) =>
			await (onEnterAsync == null ? OnEnterAsync(stateMachine) :
			Task.WhenAll(onEnterAsync(stateMachine, this), OnEnterAsync(stateMachine)));

		internal virtual async Task ExitAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) =>
			await (onExitAsync == null ? OnExitAsync(stateMachine) :
			Task.WhenAll(onExitAsync(stateMachine, this), OnExitAsync(stateMachine)));

		internal virtual async Task PauseAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) =>
			await (onPauseAsync == null ? OnPauseAsync(stateMachine) :
			Task.WhenAll(onPauseAsync(stateMachine, this), OnPauseAsync(stateMachine)));

		internal virtual async Task ResumeAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) =>
			await (onResumeAsync == null ? OnResumeAsync(stateMachine) :
			Task.WhenAll(onResumeAsync(stateMachine, this), OnResumeAsync(stateMachine)));

		internal virtual async Task SendMessageAsync(
			StateMachine<TStateId, TParamId, TMessageId> stateMachine, TMessageId message, object arg
		) { if (onMessagesAsync.ContainsKey(message)) { await onMessagesAsync[message](stateMachine, this, arg); } }

		#endregion

		#region Internal on transition or message setters used by state machine builder

		internal State<TStateId, TParamId, TMessageId> OnEnterAsync(
			StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler handler
		) {
			onEnterAsync = handler;
			return this;
		}

		internal State<TStateId, TParamId, TMessageId> OnExitAsync(
			StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler handler
		) {
			onExitAsync = handler;
			return this;
		}

		internal State<TStateId, TParamId, TMessageId> OnPauseAsync(
			StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler handler
		) {
			onPauseAsync = handler;
			return this;
		}

		internal State<TStateId, TParamId, TMessageId> OnResumeAsync(
			StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler handler
		) {
			onResumeAsync = handler;
			return this;
		}

		internal State<TStateId, TParamId, TMessageId> OnAsync(
			TMessageId message,
			StateMachine<TStateId, TParamId, TMessageId>.OnMessageAsyncHandler handler
		) {
			onMessagesAsync[message] = handler;
			return this;
		}

		#endregion

		#region Protected on transition methods available for override

		/// <summary>
		/// OnEnterAsync is called when entering this state asynchronously
		/// </summary>
		/// <param name="stateMachine">The state machine this state belongs to</param>
		protected virtual async Task OnEnterAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) =>
			await Task.CompletedTask;

		/// <summary>
		/// OnExitAsync is called when exiting this state asynchronously
		/// </summary>
		/// <param name="stateMachine">The state machine this state belongs to</param>
		protected virtual async Task OnExitAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) =>
			await Task.CompletedTask;

		/// <summary>
		/// OnPauseAsync is called when pausing this state asynchronously
		/// </summary>
		/// <param name="stateMachine">The state machine this state belongs to</param>
		protected virtual async Task OnPauseAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) =>
			await Task.CompletedTask;

		/// <summary>
		/// OnResumeAsync is called when resuming this state asynchronously
		/// </summary>
		/// <param name="stateMachine">The state machine this state belongs to</param>
		protected virtual async Task OnResumeAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) =>
			await Task.CompletedTask;

		#endregion
	}
}
