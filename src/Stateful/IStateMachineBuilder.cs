using System;
using Stateful.Utilities;

namespace Stateful {

	// Implemented by StateMachineBuilder
	public interface IStateMachineBuilder<TStateId, TParamId, TMessageId> : IAddStateAddSetParam<TStateId, TParamId, TMessageId>, IAddConditionAddHandlerAddTransitionAddStateBuildAddOr<TStateId, TParamId, TMessageId>, IAddTransitionAddExcept<TStateId, TParamId, TMessageId> { }

	// Initializer
	public interface IAddStateAddSetParam<TStateId, TParamId, TMessageId> : IAddState<TStateId, TParamId, TMessageId> {

		IAddStateAddSetParam<TStateId, TParamId, TMessageId> SetBool(TParamId param, bool value);
		IAddStateAddSetParam<TStateId, TParamId, TMessageId> SetFloat(TParamId param, float value);
		IAddStateAddSetParam<TStateId, TParamId, TMessageId> SetInt(TParamId param, int value);
		IAddStateAddSetParam<TStateId, TParamId, TMessageId> SetString(TParamId param, string value);
		IAddStateAddSetParam<TStateId, TParamId, TMessageId> SetTrigger(TParamId param);
	}

	public interface IAddState<TStateId, TParamId, TMessageId> {

		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> AddState(TStateId name, State<TStateId, TParamId, TMessageId> state);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> AddState(TStateId name, IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> builder);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> AddState(TStateId name);
	}

	public partial interface IAddTransition<TStateId, TParamId, TMessageId> {

		IAddCondition<TStateId, TParamId, TMessageId> GoTo(TStateId state);
		IAddCondition<TStateId, TParamId, TMessageId> Push(TStateId state);
		IAddCondition<TStateId, TParamId, TMessageId> Pop { get; }
	}

	public partial interface IAddTransitionAddExcept<TStateId, TParamId, TMessageId> : IAddTransition<TStateId, TParamId, TMessageId> {

		IAddTransition<TStateId, TParamId, TMessageId> Except(params TStateId[] states);
	}

	public partial interface IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> : IAddTransition<TStateId, TParamId, TMessageId>, IAddState<TStateId, TParamId, TMessageId> {

		#region Handler setters

		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> On(TMessageId id, Action action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> On(TMessageId id, Action<StateMachine<TStateId, TParamId, TMessageId>> action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> On(TMessageId id, Action<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>> action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> On(TMessageId id, StateMachine<TStateId, TParamId, TMessageId>.OnMessageHandler action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> On<T>(TMessageId id, Func<T> action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> On<T>(TMessageId id, Func<StateMachine<TStateId, TParamId, TMessageId>, T> action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> On<T>(TMessageId id, Func<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>, T> action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> On<T>(TMessageId id, StateMachine<TStateId, TParamId, TMessageId>.OnMessageHandler<T> action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnEnter(Action action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnEnter(Action<StateMachine<TStateId, TParamId, TMessageId>> action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnEnter(StateMachine<TStateId, TParamId, TMessageId>.OnTransitionHandler action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnExit(Action action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnExit(Action<StateMachine<TStateId, TParamId, TMessageId>> action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnExit(StateMachine<TStateId, TParamId, TMessageId>.OnTransitionHandler action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnPause(Action action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnPause(Action<StateMachine<TStateId, TParamId, TMessageId>> action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnPause(StateMachine<TStateId, TParamId, TMessageId>.OnTransitionHandler action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnResume(Action action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnResume(Action<StateMachine<TStateId, TParamId, TMessageId>> action);
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnResume(StateMachine<TStateId, TParamId, TMessageId>.OnTransitionHandler action);

		#endregion

		IAddTransition<TStateId, TParamId, TMessageId> From(params TStateId[] states);
		IAddTransitionAddExcept<TStateId, TParamId, TMessageId> FromAny { get; }
		StateMachine<TStateId, TParamId, TMessageId> Build(); // Finalizer
		TStateMachine Build<TStateMachine>() where TStateMachine : StateMachine<TStateId, TParamId, TMessageId>, new();
	}

	public interface IAddConditionAddHandlerAddTransitionAddStateBuildAddOr<TStateId, TParamId, TMessageId> : IAddCondition<TStateId, TParamId, TMessageId>, IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> {

		IAddCondition<TStateId, TParamId, TMessageId> Or { get; }
	}

	public interface IAddCondition<TStateId, TParamId, TMessageId> {

		IAddConditionAddHandlerAddTransitionAddStateBuildAddOr<TStateId, TParamId, TMessageId> WhenBool(TParamId param, Func<bool, bool> check);
		IAddConditionAddHandlerAddTransitionAddStateBuildAddOr<TStateId, TParamId, TMessageId> WhenFloat(TParamId param, Func<float, bool> check);
		IAddConditionAddHandlerAddTransitionAddStateBuildAddOr<TStateId, TParamId, TMessageId> WhenInt(TParamId param, Func<int, bool> check);
		IAddConditionAddHandlerAddTransitionAddStateBuildAddOr<TStateId, TParamId, TMessageId> WhenString(TParamId param, Func<string, bool> check);
		IAddConditionAddHandlerAddTransitionAddStateBuildAddOr<TStateId, TParamId, TMessageId> WhenTrigger(TParamId param);
		IAddConditionAddHandlerAddTransitionAddStateBuildAddOr<TStateId, TParamId, TMessageId> When<T>(Observable<T> param, Func<T, bool> check);
	}
}
