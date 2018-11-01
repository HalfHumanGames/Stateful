using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateMachineNet {

	public partial class State<TStateId, TParamId, TMessageId> {

		protected StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler onEnterAsync;
		protected StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler onExitAsync;
		protected StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler onPauseAsync;
		protected StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler onResumeAsync;
		protected Dictionary<TMessageId, StateMachine<TStateId, TParamId, TMessageId>.OnMessageAsyncHandler> onMessagesAsync =
			new Dictionary<TMessageId, StateMachine<TStateId, TParamId, TMessageId>.OnMessageAsyncHandler>();

		#region transition action wrappers

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

		#region On transition methods

		/// <summary>
		/// OnEnter is called when entering this state
		/// </summary>
		/// <param name="stateMachine">The state machine this state belongs to</param>
		protected virtual async Task OnEnterAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) =>
			await Task.CompletedTask;

		/// <summary>
		/// OnExit is called when exiting this state
		/// </summary>
		/// <param name="stateMachine">The state machine this state belongs to</param>
		protected virtual async Task OnExitAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) =>
			await Task.CompletedTask;

		/// <summary>
		/// OnPause is called when pausing this state
		/// </summary>
		/// <param name="stateMachine">The state machine this state belongs to</param>
		protected virtual async Task OnPauseAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) =>
			await Task.CompletedTask;

		/// <summary>
		/// OnResume is called when resuming this state
		/// </summary>
		/// <param name="stateMachine">The state machine this state belongs to</param>
		protected virtual async Task OnResumeAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) =>
			await Task.CompletedTask;

		#endregion

		#region On transition or message setters

		/// <summary>
		/// Sets the action to invoke when entering this state
		/// </summary>
		/// <param name="action">On enter action</param>
		/// <returns>Returns this state to allow method chaining</returns>
		public State<TStateId, TParamId, TMessageId> OnEnterAsync(
			StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler handler
		) {
			onEnterAsync = handler;
			return this;
		}

		/// <summary>
		/// Sets the action to invoke when exiting this state
		/// </summary>
		/// <param name="action">On enter action</param>
		/// <returns>Returns this state to allow method chaining</returns>
		public State<TStateId, TParamId, TMessageId> OnExitAsync(
			StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler handler
		) {
			onExitAsync = handler;
			return this;
		}

		/// <summary>
		/// Sets the action to invoke when pausing this state
		/// </summary>
		/// <param name="action">On pause action</param>
		/// <returns>Returns this state to allow method chaining</returns>
		public State<TStateId, TParamId, TMessageId> OnPauseAsync(
			StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler handler
		) {
			onPauseAsync = handler;
			return this;
		}

		/// <summary>
		/// Sets the action to invoke when resuming this state
		/// </summary>
		/// <param name="action">On resume action</param>
		/// <returns>Returns this state to allow method chaining</returns>
		public State<TStateId, TParamId, TMessageId> OnResumeAsync(
			StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler handler
		) {
			onResumeAsync = handler;
			return this;
		}

		public State<TStateId, TParamId, TMessageId> OnAsync(
			TMessageId message,
			StateMachine<TStateId, TParamId, TMessageId>.OnMessageAsyncHandler handler
		) {
			onMessagesAsync[message] = handler;
			return this;
		}

		#endregion
	}
}
