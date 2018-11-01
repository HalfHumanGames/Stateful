using System;
using System.Collections.Generic;
using System.Linq;
using StateMachineNet.Utilities;

namespace StateMachineNet {

	[Serializable] 
	internal abstract partial class Transition<TStateId, TParamId, TMessageId> {

		// Parameters this transition considers
		private HashSet<TParamId> parameters = new HashSet<TParamId>();

		// Parameter checks
		private List<Tuple<TParamId, Func<bool, bool>>> boolChecks =
			new List<Tuple<TParamId, Func<bool, bool>>>();
		private List<Tuple<TParamId, Func<float, bool>>> floatChecks =
			new List<Tuple<TParamId, Func<float, bool>>>();
		private List<Tuple<TParamId, Func<int, bool>>> intChecks =
			new List<Tuple<TParamId, Func<int, bool>>>();
		private List<Tuple<TParamId, Func<string, bool>>> stringChecks =
			new List<Tuple<TParamId, Func<string, bool>>>();
		private HashSet<TParamId> triggerChecks = new HashSet<TParamId>();

		// Observable management
		private Dictionary<int, bool> observableChecks = new Dictionary<int, bool>();
		private List<Action> observableRefreshes = new List<Action>();
		private List<Action> observableSubscriptions = new List<Action>();
		private List<Action> observableUnsubscriptions = new List<Action>();

		// Checks if this state considers the specifed parameter
		internal bool HasParam(TParamId param) => parameters.Contains(param);

		// Abstract methods
		internal abstract void DoTransition(StateMachine<TStateId, TParamId, TMessageId> stateMachine);
		internal abstract Transition<TStateId, TParamId, TMessageId> GetCloneWithoutChecks();

		// Used by state machine
		internal bool EvaluateTransition(StateMachine<TStateId, TParamId, TMessageId> stateMachine) =>
			!observableChecks.Any(x => !x.Value) &&
			!boolChecks.Any(x => !x.Item2(stateMachine.GetBool(x.Item1))) &&
			!floatChecks.Any(x => !x.Item2(stateMachine.GetFloat(x.Item1))) &&
			!intChecks.Any(x => !x.Item2(stateMachine.GetInt(x.Item1))) &&
			!stringChecks.Any(x => !x.Item2(stateMachine.GetString(x.Item1))) &&
			!triggerChecks.Any(x => !stateMachine.GetTrigger(x));

		#region Internal methods used by the state machine builder to add checks

		internal void AddBoolCheck(TParamId param, Func<bool, bool> check) {
			parameters.Add(param);
			boolChecks.Add(new Tuple<TParamId, Func<bool, bool>>(param, check));
		}

		internal void AddFloatCheck(TParamId param, Func<float, bool> check) {
			parameters.Add(param);
			floatChecks.Add(new Tuple<TParamId, Func<float, bool>>(param, check));
		}

		internal void AddIntCheck(TParamId param, Func<int, bool> check) {
			parameters.Add(param);
			intChecks.Add(new Tuple<TParamId, Func<int, bool>>(param, check));
		}

		internal void AddStringCheck(TParamId param, Func<string, bool> check) {
			parameters.Add(param);
			stringChecks.Add(new Tuple<TParamId, Func<string, bool>>(param, check));
		}

		internal void AddTriggerCheck(TParamId param) {
			parameters.Add(param);
			triggerChecks.Add(param);
		}

		internal void AddObservableCheck<T>(Observable<T> observable, Func<Observable<T>, bool> check) {
			int hashCode = observable.GetHashCode();
			observableChecks.Add(hashCode, check(observable));
			observableRefreshes.Add(() => observableChecks[hashCode] = check(observable));
			observableSubscriptions.Add(() => observable.ValueChanged += OnValueChanged);
			observableUnsubscriptions.Add(() => observable.ValueChanged -= OnValueChanged);
			void OnValueChanged(Observable<T> o, T p, T n) =>
				observableChecks[hashCode] = check(observable
			);
		}

		#endregion

		#region Observable utility methods used by the state machine

		internal void RefreshObservables() => observableRefreshes.ForEach(x => x());
		internal void SubscribeToObservables() => observableSubscriptions.ForEach(x => x());
		internal void UnsubscribeFromObservables() => observableUnsubscriptions.ForEach(x => x());

		#endregion
	}

	#region Concrete transitions: GoTo, Push, and Pop

	[Serializable] 
	internal partial class GoToTransition<TStateId, TParamId, TMessageId> : Transition<TStateId, TParamId, TMessageId> {

		internal TStateId state;

		internal GoToTransition(TStateId state) => this.state = state;

		internal override void DoTransition(StateMachine<TStateId, TParamId, TMessageId> stateMachine) =>
			stateMachine.GoTo(state);

		internal override Transition<TStateId, TParamId, TMessageId> GetCloneWithoutChecks() =>
			new GoToTransition<TStateId, TParamId, TMessageId>(state);
	}

	[Serializable] 
	internal partial class PushTransition<TStateId, TParamId, TMessageId> : Transition<TStateId, TParamId, TMessageId> {

		internal TStateId state;

		internal PushTransition(TStateId state) => this.state = state;

		internal override void DoTransition(StateMachine<TStateId, TParamId, TMessageId> stateMachine) =>
			stateMachine.Push(state);

		internal override Transition<TStateId, TParamId, TMessageId> GetCloneWithoutChecks() =>
			new PushTransition<TStateId, TParamId, TMessageId>(state);
	}

	[Serializable] 
	internal partial class PopTransition<TStateId, TParamId, TMessageId> : Transition<TStateId, TParamId, TMessageId> {

		internal override void DoTransition(StateMachine<TStateId, TParamId, TMessageId> stateMachine) =>
			stateMachine.Pop();

		internal override Transition<TStateId, TParamId, TMessageId> GetCloneWithoutChecks() =>
			new PopTransition<TStateId, TParamId, TMessageId>();
	}

	#endregion
}
