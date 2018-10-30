using System;
using StateMachineNet.Utilities;

namespace StateMachineNet {

	public sealed class StateMachineBuilder : StateMachineBuilder<string, string, string> {

		private StateMachineBuilder() { }

		#region Create methods

		/// <summary>
		/// Creates a new state machine builder with string ids for both states and params
		/// </summary>
		/// <returns></returns>
		public static IStateMachineBuilder<string, string, string> Create() =>
			new StateMachineBuilder();

		/// <summary>
		/// Creates a new state machine builder with string ids for states and a generic id for params
		/// </summary>
		/// <typeparam name="TStateId">The type used to identify states</typeparam>
		/// <returns></returns>
		public static IStateMachineBuilder<TStateId, string, string> Create<TStateId>() =>
			new StateMachineBuilder<TStateId, string, string>();

		/// <summary>
		/// Creates a new state machine builder with generic ids for both states and params
		/// </summary>
		/// <typeparam name="TStateId">The type used to identify states</typeparam>
		/// <typeparam name="TParamId">The type used to identify params</typeparam>
		/// <returns></returns>
		public static IStateMachineBuilder<TStateId, TParamId, string> Create<TStateId, TParamId>() =>
			new StateMachineBuilder<TStateId, TParamId, string>();

		/// <summary>
		/// Creates a new state machine builder with generic ids for both states and params
		/// </summary>
		/// <typeparam name="TStateId">The type used to identify states</typeparam>
		/// <typeparam name="TParamId">The type used to identify params</typeparam>
		/// <returns></returns>
		public static IStateMachineBuilder<TStateId, TParamId, TMessageId> Create<TStateId, TParamId, TMessageId>() =>
			new StateMachineBuilder<TStateId, TParamId, TMessageId>();

		#endregion
	}

	public partial class StateMachineBuilder<TStateId, TParamId, TMessageId> : IStateMachineBuilderFluentInterface<TStateId, TParamId, TMessageId> {

		private bool isGlobalTransition;
		private TStateId[] statesToAddTransitionsTo;
		private Transition<TStateId, TParamId, TMessageId> mostRecentlyAddedTransition;

		internal StateMachineBuilder() { }

		#region Set default parameter values

		public IStateMachineBuilder<TStateId, TParamId, TMessageId> SetBool(TParamId param, bool value) {
			Build.SetBool(param, value);
			return this;
		}
		
		public IStateMachineBuilder<TStateId, TParamId, TMessageId> SetFloat(TParamId param, float value) {
			Build.SetFloat(param, value);
			return this;
		}
		
		public IStateMachineBuilder<TStateId, TParamId, TMessageId> SetInt(TParamId param, int value) {
			Build.SetInt(param, value);
			return this;
		}

		public IStateMachineBuilder<TStateId, TParamId, TMessageId> SetString(TParamId param, string value) {
			Build.SetString(param, value);
			return this;
		}

		public IStateMachineBuilder<TStateId, TParamId, TMessageId> SetTrigger(TParamId param) {
			Build.SetTrigger(param);
			return this;
		}

		#endregion

		#region Add states or transition sources

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> AddState(TStateId name, State<TStateId, TParamId, TMessageId> state) {
			isGlobalTransition = false;
			statesToAddTransitionsTo = new TStateId[] { name };
			Build.AddState(name, state);
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> AddState(TStateId name) =>
			AddState(name, new State<TStateId, TParamId, TMessageId>());

		public IAddTransition<TStateId, TParamId, TMessageId> From(params TStateId[] states) {
			isGlobalTransition = false;
			statesToAddTransitionsTo = states;
			return this;
		}

		public IAddTransition<TStateId, TParamId, TMessageId> FromAny {
			get {
				isGlobalTransition = true;
				return this;
			}
		}

		#endregion

		#region Add handlers

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> On(TMessageId id, Action action) { 
			Build.GetState(statesToAddTransitionsTo[0]).On(id, (stateMachine, state, arg) => action());
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> On(TMessageId id, Action<StateMachine<TStateId, TParamId, TMessageId>> action) { 
			Build.GetState(statesToAddTransitionsTo[0]).On(id, (stateMachine, state, arg) => action(stateMachine));
			return this;
		}


		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> On(TMessageId id, Action<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>> action) { 
			Build.GetState(statesToAddTransitionsTo[0]).On(id, (stateMachine, state, arg) => action(stateMachine, state));
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> On(TMessageId id, Action<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>, object> action) { 
			Build.GetState(statesToAddTransitionsTo[0]).On(id, action);
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnEnter(Action action) { 
			Build.GetState(statesToAddTransitionsTo[0]).OnEnter((stateMachine, state) => action());
			return this;
		}
		
		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnEnter(Action<StateMachine<TStateId, TParamId, TMessageId>> action) { 
			Build.GetState(statesToAddTransitionsTo[0]).OnEnter((stateMachine, state) => action(stateMachine));
			return this;
		}
		
		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnEnter(Action<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>> action) { 
			Build.GetState(statesToAddTransitionsTo[0]).OnEnter(action);
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnExit(Action action) { 
			Build.GetState(statesToAddTransitionsTo[0]).OnExit((stateMachine, state) => action());
			return this;
		}
		
		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnExit(Action<StateMachine<TStateId, TParamId, TMessageId>> action) { 
			Build.GetState(statesToAddTransitionsTo[0]).OnExit((stateMachine, state) => action(stateMachine));
			return this;
		}
		
		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnExit(Action<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>> action) { 
			Build.GetState(statesToAddTransitionsTo[0]).OnExit(action);
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnPause(Action action) { 
			Build.GetState(statesToAddTransitionsTo[0]).OnPause((stateMachine, state) => action());
			return this;
		}		
		
		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnPause(Action<StateMachine<TStateId, TParamId, TMessageId>> action) { 
			Build.GetState(statesToAddTransitionsTo[0]).OnPause((stateMachine, state) => action(stateMachine));
			return this;
		}		
		
		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnPause(Action<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>> action) { 
			Build.GetState(statesToAddTransitionsTo[0]).OnPause(action);
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnResume(Action action) { 
			Build.GetState(statesToAddTransitionsTo[0]).OnResume((stateMachine, state) => action());
			return this;
		}
	
		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnResume(Action<StateMachine<TStateId, TParamId, TMessageId>> action) { 
			Build.GetState(statesToAddTransitionsTo[0]).OnResume((stateMachine, state) => action(stateMachine));
			return this;
		}
	
		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnResume(Action<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>> action) { 
			Build.GetState(statesToAddTransitionsTo[0]).OnResume(action);
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
			if (isGlobalTransition) {
				Build.AddGlobalTransition(transition);
			} else {
				foreach (TStateId state in statesToAddTransitionsTo) {
					Build.AddTransition(state, transition);
				}
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

		public IAddConditionAddHandlerAddTransitionAddStateBuildAddOr<TStateId, TParamId, TMessageId> When<T>(Observable<T> observable, Func<Observable<T>, bool> check) {
			Build.AddObservable(statesToAddTransitionsTo, observable);
			mostRecentlyAddedTransition.AddObservableCheck(observable, check);
			return this;
		}

		public IAddCondition<TStateId, TParamId, TMessageId> Or {
			get {
				AddTransition(mostRecentlyAddedTransition.GetCloneWithoutChecks());
				return this;
			}
		}

		#endregion

		public StateMachine<TStateId, TParamId, TMessageId> Build { get; } = new StateMachine<TStateId, TParamId, TMessageId>();
	}
}
