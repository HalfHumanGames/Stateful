using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateMachineNet {

	public partial class State<TStateId, TParamId, TMessageId> {

		protected Func<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>, Task> onEnterAsync;
		protected Func<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>, Task> onExitAsync;
		protected Func<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>, Task> onPauseAsync;
		protected Func<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>, Task> onResumeAsync;
		protected Dictionary<TMessageId, Func<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>, object, Task>> onMessagesAsync =
			new Dictionary<TMessageId, Func<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>, object, Task>>();

		#region transition action wrappers

		internal virtual async Task EnterAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) =>
			await (onEnterAsync == null ? OnEnterAsync(stateMachine) : Task.WhenAll(onEnterAsync(stateMachine, this), OnEnterAsync(stateMachine)));

		internal virtual async Task ExitAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) =>
			await (onExitAsync == null ? OnExitAsync(stateMachine) : Task.WhenAll(onExitAsync(stateMachine, this), OnExitAsync(stateMachine)));

		internal virtual async Task PauseAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) =>
			await (onPauseAsync == null ? OnPauseAsync(stateMachine) : Task.WhenAll(onPauseAsync(stateMachine, this), OnPauseAsync(stateMachine)));

		internal virtual async Task ResumeAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) =>
			await (onResumeAsync == null ? OnResumeAsync(stateMachine) : Task.WhenAll(onResumeAsync(stateMachine, this), OnResumeAsync(stateMachine)));

		internal virtual async Task SendMessageAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine, TMessageId message, object arg) {
			if (onMessagesAsync.ContainsKey(message)) { await onMessagesAsync[message](stateMachine, this, arg); }
		}

		#endregion

		#region On transition methods

		/// <summary>
		/// OnEnter is called when entering this state
		/// </summary>
		/// <param name="stateMachine">The state machine this state belongs to</param>
		protected virtual async Task OnEnterAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) => await Task.CompletedTask;

		/// <summary>
		/// OnExit is called when exiting this state
		/// </summary>
		/// <param name="stateMachine">The state machine this state belongs to</param>
		protected virtual async Task OnExitAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) => await Task.CompletedTask;

		/// <summary>
		/// OnPause is called when pausing this state
		/// </summary>
		/// <param name="stateMachine">The state machine this state belongs to</param>
		protected virtual async Task OnPauseAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) => await Task.CompletedTask;

		/// <summary>
		/// OnResume is called when resuming this state
		/// </summary>
		/// <param name="stateMachine">The state machine this state belongs to</param>
		protected virtual async Task OnResumeAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) => await Task.CompletedTask;

		#endregion

		#region On transition or message setters

		/// <summary>
		/// Sets the action to invoke when entering this state
		/// </summary>
		/// <param name="action">On enter action</param>
		/// <returns>Returns this state to allow method chaining</returns>
		public State<TStateId, TParamId, TMessageId> OnEnterAsync(
			Func<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>, Task> action
		) {
			onEnterAsync = action;
			return this;
		}

		/// <summary>
		/// Sets the action to invoke when exiting this state
		/// </summary>
		/// <param name="action">On enter action</param>
		/// <returns>Returns this state to allow method chaining</returns>
		public State<TStateId, TParamId, TMessageId> OnExitAsync(
			Func<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>, Task> action
		) {
			onExitAsync = action;
			return this;
		}

		/// <summary>
		/// Sets the action to invoke when pausing this state
		/// </summary>
		/// <param name="action">On pause action</param>
		/// <returns>Returns this state to allow method chaining</returns>
		public State<TStateId, TParamId, TMessageId> OnPauseAsync(
			Func<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>, Task> action
		) {
			onPauseAsync = action;
			return this;
		}

		/// <summary>
		/// Sets the action to invoke when resuming this state
		/// </summary>
		/// <param name="action">On resume action</param>
		/// <returns>Returns this state to allow method chaining</returns>
		public State<TStateId, TParamId, TMessageId> OnResumeAsync(
			Func<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>, Task> action
		) {
			onResumeAsync = action;
			return this;
		}

		public State<TStateId, TParamId, TMessageId> OnAsync(
			TMessageId message,
			Func<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>, object, Task> action
		) {
			onMessagesAsync[message] = action;
			return this;
		}

		#endregion
	}
}
