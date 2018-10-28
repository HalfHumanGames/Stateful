using System;
using System.Collections.Generic;
using System.Linq;
using StateMachineNet.Utilities;

namespace StateMachineNet {

	internal abstract class Transition<TStateId, TParamId> {

		private HashSet<TParamId> parameters = new HashSet<TParamId>();
		private List<Tuple<TParamId, Func<bool, bool>>> boolChecks = new List<Tuple<TParamId, Func<bool, bool>>>();
		private List<Tuple<TParamId, Func<float, bool>>> floatChecks = new List<Tuple<TParamId, Func<float, bool>>>();
		private List<Tuple<TParamId, Func<int, bool>>> intChecks = new List<Tuple<TParamId, Func<int, bool>>>();
		private List<Tuple<TParamId, Func<string, bool>>> stringChecks = new List<Tuple<TParamId, Func<string, bool>>>();
		private HashSet<TParamId> triggerChecks = new HashSet<TParamId>();
		private Dictionary<int, bool> observableChecks = new Dictionary<int, bool>();
		private List<Action> observableRefreshes = new List<Action>();
		private List<Action> observableSubscriptions = new List<Action>();
		private List<Action> observableUnsubscriptions = new List<Action>();

		internal abstract void DoTransition(StateMachine<TStateId, TParamId> stateMachine);
		internal abstract Transition<TStateId, TParamId> GetCloneWithoutChecks();

		internal bool HasParam(TParamId param) => parameters.Contains(param);

		internal bool EvaluateTransition(StateMachine<TStateId, TParamId> stateMachine) =>
			!boolChecks.Any(x => !x.Item2(stateMachine.GetBool(x.Item1))) &&
			!floatChecks.Any(x => !x.Item2(stateMachine.GetFloat(x.Item1))) &&
			!intChecks.Any(x => !x.Item2(stateMachine.GetInt(x.Item1))) &&
			!stringChecks.Any(x => !x.Item2(stateMachine.GetString(x.Item1))) &&
			!triggerChecks.Any(x => !stateMachine.GetTrigger(x));

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
			void OnValueChanged(Observable<T> o, T p, T n) => observableChecks[hashCode] = check(observable);
		}
		internal void RefreshObservables() => observableRefreshes.ForEach(x => x());
		internal void SubscribeToObservables() => observableSubscriptions.ForEach(x => x());
		internal void UnsubscribeFromObservables() => observableUnsubscriptions.ForEach(x => x());
	}

	internal class GoToTransition<TStateId, TParamId> : Transition<TStateId, TParamId> {

		internal TStateId State;

		internal GoToTransition(TStateId state) => State = state;

		internal override void DoTransition(StateMachine<TStateId, TParamId> stateMachine) =>
			stateMachine.GoTo(State);

		internal override Transition<TStateId, TParamId> GetCloneWithoutChecks() =>
			new GoToTransition<TStateId, TParamId>(State);
	}

	internal class PushTransition<TStateId, TParamId> : Transition<TStateId, TParamId> {

		internal TStateId State;

		internal PushTransition(TStateId state) => State = state;

		internal override void DoTransition(StateMachine<TStateId, TParamId> stateMachine) =>
			stateMachine.Push(State);

		internal override Transition<TStateId, TParamId> GetCloneWithoutChecks() =>
			new PushTransition<TStateId, TParamId>(State);
	}

	internal class PopTransition<TStateId, TParamId> : Transition<TStateId, TParamId> {

		internal override void DoTransition(StateMachine<TStateId, TParamId> stateMachine) =>
			stateMachine.Pop();

		internal override Transition<TStateId, TParamId> GetCloneWithoutChecks() =>
			new PopTransition<TStateId, TParamId>();
	}
}
