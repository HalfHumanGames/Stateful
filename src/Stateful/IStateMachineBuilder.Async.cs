#if TASKS

using System;
using System.Threading.Tasks;

namespace Stateful {

	public partial interface IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> : IAddTransition<TStateId, TParamId, TMessageId>, IAddState<TStateId, TParamId, TMessageId> {

		#region Handler setters

		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnAsync(TMessageId id, Func<Task> action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnAsync(TMessageId id, Func<StateMachine<TStateId, TParamId, TMessageId>, Task> action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnAsync(TMessageId id, Func<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>, Task> action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnAsync(TMessageId id, StateMachine<TStateId, TParamId, TMessageId>.OnMessageAsyncHandler action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnAsync<T>(TMessageId id, Func<Task<T>> action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnAsync<T>(TMessageId id, Func<StateMachine<TStateId, TParamId, TMessageId>, Task<T>> action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnAsync<T>(TMessageId id, Func<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>, Task<T>> action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnAsync<T>(TMessageId id, StateMachine<TStateId, TParamId, TMessageId>.OnMessageAsyncHandler<T> action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnEnterAsync(Func<Task> action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnEnterAsync(Func<StateMachine<TStateId, TParamId, TMessageId>, Task> action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnEnterAsync(StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnExitAsync(Func<Task> action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnExitAsync(Func<StateMachine<TStateId, TParamId, TMessageId>, Task> action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnExitAsync(StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnPauseAsync(Func<Task> action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnPauseAsync(Func<StateMachine<TStateId, TParamId, TMessageId>, Task> action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnPauseAsync(StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnResumeAsync(Func<Task> action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnResumeAsync(Func<StateMachine<TStateId, TParamId, TMessageId>, Task> action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnResumeAsync(StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler action);

		#endregion
	}
}

#endif
