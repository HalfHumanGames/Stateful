using System;
using StateMachineNet.Utilities;

namespace StateMachineNet {

	public sealed class StateMachineBuilder : StateMachineBuilder<string, string> {

		private StateMachineBuilder() { }

		#region Create methods

		/// <summary>
		/// Creates a new state machine builder with string ids for both states and params
		/// </summary>
		/// <returns></returns>
		public static IStateMachineBuilder<string, string> Create() =>
			new StateMachineBuilder();

		/// <summary>
		/// Creates a new state machine builder with string ids for states and a generic id for params
		/// </summary>
		/// <typeparam name="TStateId">The type used to identify states</typeparam>
		/// <returns></returns>
		public static IStateMachineBuilder<TStateId, string> Create<TStateId>() =>
			new StateMachineBuilder<TStateId, string>();

		/// <summary>
		/// Creates a new state machine builder with generic ids for both states and params
		/// </summary>
		/// <typeparam name="TStateId">The type used to identify states</typeparam>
		/// <typeparam name="TParamId">The type used to identify params</typeparam>
		/// <returns></returns>
		public static IStateMachineBuilder<TStateId, TParamId> Create<TStateId, TParamId>() =>
			new StateMachineBuilder<TStateId, TParamId>();

		#endregion
	}

	public class StateMachineBuilder<TStateId, TParamId> : IStateMachineBuilderFluentInterface<TStateId, TParamId> {

		private bool isGlobalTransition;
		private TStateId[] statesToAddTransitionsTo;
		private Transition<TStateId, TParamId> mostRecentlyAddedTransition;

		internal StateMachineBuilder() { }

		#region Set default parameter values

		public IStateMachineBuilder<TStateId, TParamId> SetBool(TParamId param, bool value) {
			Build.SetBool(param, value);
			return this;
		}
		
		public IStateMachineBuilder<TStateId, TParamId> SetFloat(TParamId param, float value) {
			Build.SetFloat(param, value);
			return this;
		}
		
		public IStateMachineBuilder<TStateId, TParamId> SetInt(TParamId param, int value) {
			Build.SetInt(param, value);
			return this;
		}

		public IStateMachineBuilder<TStateId, TParamId> SetString(TParamId param, string value) {
			Build.SetString(param, value);
			return this;
		}

		public IStateMachineBuilder<TStateId, TParamId> SetTrigger(TParamId param) {
			Build.SetTrigger(param);
			return this;
		}

		#endregion

		#region Add states or transition sources

		public IAddTransitionAddStateBuild<TStateId, TParamId> AddState(TStateId name, State<TStateId, TParamId> state) {
			isGlobalTransition = false;
			statesToAddTransitionsTo = new TStateId[] { name };
			Build.AddState(name, state);
			return this;
		}

		public IAddTransitionAddStateBuild<TStateId, TParamId> AddState(TStateId name) =>
			AddState(name, new State<TStateId, TParamId>());

		public IAddTransition<TStateId, TParamId> From(params TStateId[] states) {
			isGlobalTransition = false;
			statesToAddTransitionsTo = states;
			return this;
		}

		public IAddTransition<TStateId, TParamId> FromAny {
			get {
				isGlobalTransition = true;
				return this;
			}
		}

		#endregion

		#region Add transitions

		public IAddCondition<TStateId, TParamId> GoTo(TStateId state) {
			AddTransition(new GoToTransition<TStateId, TParamId>(state));
			return this;
		}

		public IAddCondition<TStateId, TParamId> Push(TStateId state) {
			AddTransition(new PushTransition<TStateId, TParamId>(state));
			return this;
		}

		public IAddCondition<TStateId, TParamId> Pop {
			get {
				AddTransition(new PopTransition<TStateId, TParamId>());
				return this;
			}
		}

		private void AddTransition(Transition<TStateId, TParamId> transition) {
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

		public IAddConditionAddTransitionAddStateBuildAddOr<TStateId, TParamId> WhenBool(TParamId param, Func<bool, bool> check) {
			mostRecentlyAddedTransition.AddBoolCheck(param, check);
			return this;
		}

		public IAddConditionAddTransitionAddStateBuildAddOr<TStateId, TParamId> WhenFloat(TParamId param, Func<float, bool> check) {
			mostRecentlyAddedTransition.AddFloatCheck(param, check);
			return this;
		}

		public IAddConditionAddTransitionAddStateBuildAddOr<TStateId, TParamId> WhenInt(TParamId param, Func<int, bool> check) {
			mostRecentlyAddedTransition.AddIntCheck(param, check);
			return this;
		}

		public IAddConditionAddTransitionAddStateBuildAddOr<TStateId, TParamId> WhenString(TParamId param, Func<string, bool> check) {
			mostRecentlyAddedTransition.AddStringCheck(param, check);
			return this;
		}

		public IAddConditionAddTransitionAddStateBuildAddOr<TStateId, TParamId> WhenTrigger(TParamId param) {
			mostRecentlyAddedTransition.AddTriggerCheck(param);
			return this;
		}

		public IAddConditionAddTransitionAddStateBuildAddOr<TStateId, TParamId> When<T>(Observable<T> observable, Func<Observable<T>, bool> check) {
			Build.AddObservable(statesToAddTransitionsTo, observable);
			mostRecentlyAddedTransition.AddObservableCheck(observable, check);
			return this;
		}

		public IAddTransitionAddStateBuild<TStateId, TParamId> On<T>(T id, Action<StateMachine<TStateId, TParamId>> action) {
			int hashCode = id.GetHashCode();
			return this;
		}

		public IAddCondition<TStateId, TParamId> Or {
			get {
				AddTransition(mostRecentlyAddedTransition.GetCloneWithoutChecks());
				return this;
			}
		}

		#endregion

		public StateMachine<TStateId, TParamId> Build { get; } = new StateMachine<TStateId, TParamId>();
	}
}
