using System;
using System.Collections.Generic;
using System.Linq;
using Stateful.Utilities;

namespace Stateful {

	internal abstract partial class Transition<TStateId, TParamId, TMessageId> {

		// Parameters this transition considers
		private HashSet<int> paramHashCodes = new HashSet<int>();

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
		private List<Func<bool>> observableChecks = new List<Func<bool>>();

		// Checks if this state considers a specific parameter or observable
		internal bool HasParam(int hashCode) => paramHashCodes.Contains(hashCode);

		// Abstract methods
		internal abstract void DoTransition(StateMachine<TStateId, TParamId, TMessageId> stateMachine);
		internal abstract Transition<TStateId, TParamId, TMessageId> GetCloneWithoutChecks();

		// Used by state machine
		internal bool EvaluateTransition(StateMachine<TStateId, TParamId, TMessageId> stateMachine) =>
			!boolChecks.Any(x => !x.Item2(stateMachine.GetBool(x.Item1))) &&
			!floatChecks.Any(x => !x.Item2(stateMachine.GetFloat(x.Item1))) &&
			!intChecks.Any(x => !x.Item2(stateMachine.GetInt(x.Item1))) &&
			!stringChecks.Any(x => !x.Item2(stateMachine.GetString(x.Item1))) &&
			!triggerChecks.Any(x => !stateMachine.GetTrigger(x)) &&
			!observableChecks.Any(x => !x());

		#region Internal methods used by the state machine builder to add checks

		internal void AddBoolCheck(TParamId param, Func<bool, bool> check) {
			paramHashCodes.Add(param.GetHashCode());
			boolChecks.Add(new Tuple<TParamId, Func<bool, bool>>(param, check));
		}

		internal void AddFloatCheck(TParamId param, Func<float, bool> check) {
			paramHashCodes.Add(param.GetHashCode());
			floatChecks.Add(new Tuple<TParamId, Func<float, bool>>(param, check));
		}

		internal void AddIntCheck(TParamId param, Func<int, bool> check) {
			paramHashCodes.Add(param.GetHashCode());
			intChecks.Add(new Tuple<TParamId, Func<int, bool>>(param, check));
		}

		internal void AddStringCheck(TParamId param, Func<string, bool> check) {
			paramHashCodes.Add(param.GetHashCode());
			stringChecks.Add(new Tuple<TParamId, Func<string, bool>>(param, check));
		}

		internal void AddTriggerCheck(TParamId param) {
			paramHashCodes.Add(param.GetHashCode());
			triggerChecks.Add(param);
		}

		internal void AddObservableCheck(IObservable observable, Func<bool> check) {
			paramHashCodes.Add(observable.GetHashCode());
			observableChecks.Add(check);
		}

		#endregion
	}

	#region Concrete transitions: GoTo, Push, and Pop

	internal partial class GoToTransition<TStateId, TParamId, TMessageId> : Transition<TStateId, TParamId, TMessageId> {

		internal TStateId state;

		internal GoToTransition(TStateId state) => this.state = state;

		internal override void DoTransition(StateMachine<TStateId, TParamId, TMessageId> stateMachine) =>
			stateMachine.GoTo(state);

		internal override Transition<TStateId, TParamId, TMessageId> GetCloneWithoutChecks() =>
			new GoToTransition<TStateId, TParamId, TMessageId>(state);
	}

	internal partial class PushTransition<TStateId, TParamId, TMessageId> : Transition<TStateId, TParamId, TMessageId> {

		internal TStateId state;

		internal PushTransition(TStateId state) => this.state = state;

		internal override void DoTransition(StateMachine<TStateId, TParamId, TMessageId> stateMachine) =>
			stateMachine.Push(state);

		internal override Transition<TStateId, TParamId, TMessageId> GetCloneWithoutChecks() =>
			new PushTransition<TStateId, TParamId, TMessageId>(state);
	}

	internal partial class PopTransition<TStateId, TParamId, TMessageId> : Transition<TStateId, TParamId, TMessageId> {

		internal override void DoTransition(StateMachine<TStateId, TParamId, TMessageId> stateMachine) =>
			stateMachine.Pop();

		internal override Transition<TStateId, TParamId, TMessageId> GetCloneWithoutChecks() =>
			new PopTransition<TStateId, TParamId, TMessageId>();
	}

	#endregion
}
