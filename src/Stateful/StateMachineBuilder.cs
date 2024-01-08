using System;
using System.Linq;
using Stateful.Utilities;

namespace Stateful {

	public class StateMachineBuilder : StateMachineBuilder<string, string, string> {

		private StateMachineBuilder() { }

		#region Create methods

		public static IAddStateAddSetParam<string, string, string> Create() => new StateMachineBuilder();
		public static IAddStateAddSetParam<TStateId, string, string> Create<TStateId>() => new StateMachineBuilder<TStateId, string, string>();
		public static IAddStateAddSetParam<TStateId, TParamId, string> Create<TStateId, TParamId>() => new StateMachineBuilder<TStateId, TParamId, string>();
		public static IAddStateAddSetParam<TStateId, TParamId, TMessageId> Create<TStateId, TParamId, TMessageId>() => new StateMachineBuilder<TStateId, TParamId, TMessageId>();

		#endregion
	}

	public partial class StateMachineBuilder<TStateId, TParamId, TMessageId> : IStateMachineBuilder<TStateId, TParamId, TMessageId> {

		private TStateId[] statesToAddTransitionsTo;
		private Transition<TStateId, TParamId, TMessageId> mostRecentlyAddedTransition;

		internal StateMachineBuilder() { }

		#region Set default parameter values

		public IAddStateAddSetParam<TStateId, TParamId, TMessageId> SetBool(TParamId param, bool value) {
			stateMachine.SetBool(param, value);
			return this;
		}

		public IAddStateAddSetParam<TStateId, TParamId, TMessageId> SetFloat(TParamId param, float value) {
			stateMachine.SetFloat(param, value);
			return this;
		}

		public IAddStateAddSetParam<TStateId, TParamId, TMessageId> SetInt(TParamId param, int value) {
			stateMachine.SetInt(param, value);
			return this;
		}

		public IAddStateAddSetParam<TStateId, TParamId, TMessageId> SetString(TParamId param, string value) {
			stateMachine.SetString(param, value);
			return this;
		}

		public IAddStateAddSetParam<TStateId, TParamId, TMessageId> SetTrigger(TParamId param) {
			stateMachine.SetTrigger(param);
			return this;
		}

		#endregion

		#region Add states or transition sources

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> AddState(TStateId name) {
			return AddState(name, new State<TStateId, TParamId, TMessageId>());
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> AddState(TStateId name, State<TStateId, TParamId, TMessageId> state) {
			statesToAddTransitionsTo = new TStateId[] { name };
			stateMachine.AddState(name, state);
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> AddState(TStateId name, IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> builder) {
			return AddState(name, builder.Build());
		}

		public IAddTransitionAddExcept<TStateId, TParamId, TMessageId> FromAny {
			get {
				From(stateMachine.StateIds);
				return this;
			}
		}

		public IAddTransition<TStateId, TParamId, TMessageId> Except(params TStateId[] states) {
			From(stateMachine.StateIds.Except(states).ToArray());
			return this;
		}

		public IAddTransition<TStateId, TParamId, TMessageId> From(params TStateId[] states) {
			statesToAddTransitionsTo = states;
			return this;
		}

		#endregion

		#region Add handlers

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> On(TMessageId id, Action action) {
			stateMachine.GetState(statesToAddTransitionsTo[0]).On(id, (stateMachine, state, arg) => action());
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> On(TMessageId id, Action<StateMachine<TStateId, TParamId, TMessageId>> action) {
			stateMachine.GetState(statesToAddTransitionsTo[0]).On(id, (stateMachine, state, arg) => action(stateMachine));
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> On(TMessageId id, Action<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>> action) {
			stateMachine.GetState(statesToAddTransitionsTo[0]).On(id, (stateMachine, state, arg) => action(stateMachine, state));
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> On(TMessageId id, StateMachine<TStateId, TParamId, TMessageId>.OnMessageHandler action) {
			stateMachine.GetState(statesToAddTransitionsTo[0]).On(id, action);
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> On<T>(TMessageId id, Func<T> action) {
			stateMachine.GetState(statesToAddTransitionsTo[0]).On<T>(id, (stateMachine, state, arg) => action());
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> On<T>(TMessageId id, Func<StateMachine<TStateId, TParamId, TMessageId>, T> action) {
			stateMachine.GetState(statesToAddTransitionsTo[0]).On<T>(id, (stateMachine, state, arg) => action(stateMachine));
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> On<T>(TMessageId id, Func<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>, T> action) {
			stateMachine.GetState(statesToAddTransitionsTo[0]).On<T>(id, (stateMachine, state, arg) => action(stateMachine, state));
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> On<T>(TMessageId id, StateMachine<TStateId, TParamId, TMessageId>.OnMessageHandler<T> action) {
			stateMachine.GetState(statesToAddTransitionsTo[0]).On<T>(id, action);
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnEnter(Action action) {
			stateMachine.GetState(statesToAddTransitionsTo[0]).OnEnter((stateMachine, state) => action());
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnEnter(Action<StateMachine<TStateId, TParamId, TMessageId>> action) {
			stateMachine.GetState(statesToAddTransitionsTo[0]).OnEnter((stateMachine, state) => action(stateMachine));
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnEnter(StateMachine<TStateId, TParamId, TMessageId>.OnTransitionHandler action) {
			stateMachine.GetState(statesToAddTransitionsTo[0]).OnEnter(action);
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnExit(Action action) {
			stateMachine.GetState(statesToAddTransitionsTo[0]).OnExit((stateMachine, state) => action());
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnExit(Action<StateMachine<TStateId, TParamId, TMessageId>> action) {
			stateMachine.GetState(statesToAddTransitionsTo[0]).OnExit((stateMachine, state) => action(stateMachine));
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnExit(StateMachine<TStateId, TParamId, TMessageId>.OnTransitionHandler action) {
			stateMachine.GetState(statesToAddTransitionsTo[0]).OnExit(action);
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnPause(Action action) {
			stateMachine.GetState(statesToAddTransitionsTo[0]).OnPause((stateMachine, state) => action());
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnPause(Action<StateMachine<TStateId, TParamId, TMessageId>> action) {
			stateMachine.GetState(statesToAddTransitionsTo[0]).OnPause((stateMachine, state) => action(stateMachine));
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnPause(StateMachine<TStateId, TParamId, TMessageId>.OnTransitionHandler action) {
			stateMachine.GetState(statesToAddTransitionsTo[0]).OnPause(action);
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnResume(Action action) {
			stateMachine.GetState(statesToAddTransitionsTo[0]).OnResume((stateMachine, state) => action());
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnResume(Action<StateMachine<TStateId, TParamId, TMessageId>> action) {
			stateMachine.GetState(statesToAddTransitionsTo[0]).OnResume((stateMachine, state) => action(stateMachine));
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnResume(StateMachine<TStateId, TParamId, TMessageId>.OnTransitionHandler action) {
			stateMachine.GetState(statesToAddTransitionsTo[0]).OnResume(action);
			return this;
		}

		#endregion

		#region Add transitions

		public IAddCondition<TStateId, TParamId, TMessageId> GoTo(TStateId state) {
			AddTransition(new GoToTransition<TStateId, TParamId, TMessageId>(state));
			return this;
		}

		public IAddCondition<TStateId, TParamId, TMessageId> Push(TStateId state) {
			AddTransition(new PushTransition<TStateId, TParamId, TMessageId>(state));
			return this;
		}

		public IAddCondition<TStateId, TParamId, TMessageId> Pop {
			get {
				AddTransition(new PopTransition<TStateId, TParamId, TMessageId>());
				return this;
			}
		}

		private void AddTransition(Transition<TStateId, TParamId, TMessageId> transition) {
			foreach (TStateId state in statesToAddTransitionsTo) {
				stateMachine.AddTransition(state, transition);
			}
			mostRecentlyAddedTransition = transition;
		}

		#endregion

		#region Add transition conditions

		public IAddConditionAddHandlerAddTransitionAddStateBuildAddOr<TStateId, TParamId, TMessageId> WhenBool(TParamId param, Func<bool, bool> check) {
			mostRecentlyAddedTransition.AddBoolCheck(param, check);
			return this;
		}

		public IAddConditionAddHandlerAddTransitionAddStateBuildAddOr<TStateId, TParamId, TMessageId> WhenFloat(TParamId param, Func<float, bool> check) {
			mostRecentlyAddedTransition.AddFloatCheck(param, check);
			return this;
		}

		public IAddConditionAddHandlerAddTransitionAddStateBuildAddOr<TStateId, TParamId, TMessageId> WhenInt(TParamId param, Func<int, bool> check) {
			mostRecentlyAddedTransition.AddIntCheck(param, check);
			return this;
		}

		public IAddConditionAddHandlerAddTransitionAddStateBuildAddOr<TStateId, TParamId, TMessageId> WhenString(TParamId param, Func<string, bool> check) {
			mostRecentlyAddedTransition.AddStringCheck(param, check);
			return this;
		}

		public IAddConditionAddHandlerAddTransitionAddStateBuildAddOr<TStateId, TParamId, TMessageId> WhenTrigger(TParamId param) {
			mostRecentlyAddedTransition.AddTriggerCheck(param);
			return this;
		}

		public IAddConditionAddHandlerAddTransitionAddStateBuildAddOr<TStateId, TParamId, TMessageId> When<T>(Observable<T> observable, Func<T, bool> check) {
			stateMachine.AddObservable((INotifyChanged) observable, check);
			mostRecentlyAddedTransition.AddObservableCheck((INotifyChanged) observable, () => check(observable.Value));
			return this;
		}

		public IAddCondition<TStateId, TParamId, TMessageId> Or {
			get {
				AddTransition(mostRecentlyAddedTransition.GetCloneWithoutChecks());
				return this;
			}
		}

		#endregion

		private StateMachine<TStateId, TParamId, TMessageId> stateMachine = new StateMachine<TStateId, TParamId, TMessageId>();

		public StateMachine<TStateId, TParamId, TMessageId> Build() => stateMachine;

		public TStateMachine Build<TStateMachine>() where TStateMachine : StateMachine<TStateId, TParamId, TMessageId>, new() {
			return Build().As<TStateMachine>();
		}
	}
}
