#if TASKS

using System;
using System.Threading.Tasks;

namespace Stateful {

	public partial interface IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> :
		IAddTransition<TStateId, TParamId, TMessageId>,
		IAddState<TStateId, TParamId, TMessageId> {

		#region Handler setters

		/// <summary>
		/// Register a custom message handler
		/// </summary>
		/// <param name="id">Message id</param>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnAsync(TMessageId id, Func<Task> action);

		/// <summary>
		/// Register a custom message handler
		/// </summary>
		/// <param name="id">Message id</param>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnAsync(
			TMessageId id, Func<StateMachine<TStateId, TParamId, TMessageId>, Task> action
		);

		/// <summary>
		/// Register a custom message handler
		/// </summary>
		/// <param name="id">Message id</param>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnAsync(
			TMessageId id,
			Func<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>, Task> action
		);

		/// <summary>
		/// Register a custom message handler
		/// </summary>
		/// <param name="id">Message id</param>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnAsync(
			TMessageId id, StateMachine<TStateId, TParamId, TMessageId>.OnMessageAsyncHandler action
		);

		/// <summary>
		/// Register a custom message handler
		/// </summary>
		/// <param name="id">Message id</param>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnAsync<T>(TMessageId id, Func<Task<T>> action);

		/// <summary>
		/// Register a custom message handler
		/// </summary>
		/// <param name="id">Message id</param>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnAsync<T>(
			TMessageId id, Func<StateMachine<TStateId, TParamId, TMessageId>, Task<T>> action
		);

		/// <summary>
		/// Register a custom message handler
		/// </summary>
		/// <param name="id">Message id</param>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnAsync<T>(
			TMessageId id,
			Func<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>, Task<T>> action
		);

		/// <summary>
		/// Register a custom message handler
		/// </summary>
		/// <param name="id">Message id</param>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnAsync<T>(
			TMessageId id, StateMachine<TStateId, TParamId, TMessageId>.OnMessageAsyncHandler<T> action
		);

		/// <summary>
		/// Register an on enter handler
		/// </summary>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnEnterAsync(Func<Task> action);

		/// <summary>
		/// Register an on enter handler
		/// </summary>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnEnterAsync(
			Func<StateMachine<TStateId, TParamId, TMessageId>, Task> action
		);

		/// <summary>
		/// Register an on enter handler
		/// </summary>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnEnterAsync(
			StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler action
		);

		/// <summary>
		/// Register an on exit handler
		/// </summary>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnExitAsync(Func<Task> action);

		/// <summary>
		/// Register an on exit handler
		/// </summary>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnExitAsync(
			Func<StateMachine<TStateId, TParamId, TMessageId>, Task> action
		);

		/// <summary>
		/// Register an on exit handler
		/// </summary>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnExitAsync(
			StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler action
		);

		/// <summary>
		/// Register an on pause handler
		/// </summary>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnPauseAsync(Func<Task> action);

		/// <summary>
		/// Register an on pause handler
		/// </summary>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnPauseAsync(
			Func<StateMachine<TStateId, TParamId, TMessageId>, Task> action
		);

		/// <summary>
		/// Register an on pause handler
		/// </summary>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnPauseAsync(
			StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler action
		);

		/// <summary>
		/// Register an on resume handler
		/// </summary>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnResumeAsync(Func<Task> action);

		/// <summary>
		/// Register an on resume handler
		/// </summary>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnResumeAsync(
			Func<StateMachine<TStateId, TParamId, TMessageId>, Task> action
		);

		/// <summary>
		/// Register an on resume handler
		/// </summary>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnResumeAsync(
			StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler action
		);

		#endregion
	}
}

#endif
