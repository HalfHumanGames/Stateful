using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StateMachineNet {

	public partial class StateMachine<TStateId, TParamId, TMessageId> : State<TStateId, TParamId, TMessageId> {

		#region State method overrides

		internal override async Task EnterAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) {
			if (IsSubstate) {
				StateMachine<TStateId, TParamId, TMessageId> parent = this;
				do {
					parent = parent.ParentState;
					globalTransitions.AddRange(parent.globalTransitions);
				} while (parent.IsSubstate);
			}
			await base.EnterAsync(stateMachine);
			await StartAsync();
		}

		internal override async Task ExitAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) {
			await StopAsync();
			await base.ExitAsync(stateMachine);
		}

		internal override async Task SendMessageAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine, TMessageId message, object arg) {
			await base.SendMessageAsync(stateMachine, message, arg);
			await SendMessageAsync(message, arg);
		}

		#endregion

		#region Flow control

		/// <summary>
		/// Starts the state machine using the first state added during configuration
		/// </summary>
		public async Task StartAsync() {
			Log("Starting state machine.");
			IsRunning = true;
			await GoToAsync(initialStateId);
		}

		/// <summary>
		/// Stops the state machine using the state with the specified state id
		/// </summary>
		/// <param name="state">The state id of the state to start at</param>
		public async Task StartAsync(TStateId state) {
			initialStateId = state;
			await StartAsync();
		}

		/// <summary>
		/// Stops the state machine
		/// </summary>
		public async Task StopAsync() {
			Log("Stopping state machine.");
			if (activeStates.Count > 0) {
				await Task.WhenAll(states.Select(x => x.Value.ExitAsync(this)));
			}
			IsRunning = false;
		}

		/// <summary>
		/// Transitions to the specified state irregardless of transitions and transition conditions
		/// </summary>
		/// <param name="state">The state id of the state to transition to</param>
		public async Task GoToAsync(TStateId state) {
			Log($"Going to state {state}.");
			if (!states.ContainsKey(state)) {
				if (IsSubstate) {
					Log($"State {state} not found, searching parents for state.");
					StateMachine<TStateId, TParamId, TMessageId> parent = this;
					do {
						parent = parent.ParentState;
						if (parent.states.ContainsKey(state)) {
							await parent.GoToAsync(state);
						}
					} while (parent.IsSubstate);
				}
				throw new ArgumentException($"State {state} does not exist.");
			}
			transitionLock.AddLock();
			if (activeStates.Count > 0) {
				TStateId popped = activeStates.Pop();
				UnsubscribeFromObservables(popped);
				await states[popped].ExitAsync(this);
			}
			activeStates.Push(state);
			SubscribeToObservables(state);
			await states[state].EnterAsync(this);
			transitionLock.RemoveLock();
			await EvaluateTransitionsAsync();
		}

		/// <summary>
		/// Pushes the specified state onto the active states stack irregardless of transitions and transition conditions
		/// </summary>
		/// <param name="state">The state id of the state to push onto the active states stack</param>
		public async Task PushAsync(TStateId state) {
			Log($"Pushing state {state}.");
			if (!states.ContainsKey(state)) {
				throw new ArgumentException($"State {state} does not exist.");
			}
			transitionLock.AddLock();
			if (activeStates.Count > 0) {
				await states[activeStates.Peek()].PauseAsync(this);
			}
			activeStates.Push(state);
			SubscribeToObservables(state);
			await states[activeStates.Peek()].EnterAsync(this);
			transitionLock.RemoveLock();
			await EvaluateTransitionsAsync();
		}

		/// <summary>
		/// Pops the peek state of the active states stack irregardless of transitions and transition conditions
		/// </summary>
		public async Task PopAsync() {
			Log($"Popping state {ActiveStateId}.");
			transitionLock.AddLock();
			if (activeStates.Count > 0) {
				TStateId popped = activeStates.Pop();
				UnsubscribeFromObservables(popped);
				await states[popped].ExitAsync(this);
			}
			if (activeStates.Count > 0) {
				await states[activeStates.Peek()].ResumeAsync(this);
			}
			transitionLock.RemoveLock();
			await EvaluateTransitionsAsync();
		}

		#endregion

		#region Set parameters

		/// <summary>
		/// Sets an bool parameter value
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <param name="value">Value</param>
		public async Task SetBoolAsync(TParamId param, bool value) {
			Log($"Setting bool {param} to {value}.");
			if (bools.ContainsKey(param) && bools[param] == value) {
				Log($"Bool {param} already equals {value}.");
				return;
			}
			bools[param] = value;
			await EvaluateTransitionsAsync(param);
		}

		/// <summary>
		/// Sets an float parameter value
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <param name="value">Value</param>
		public async Task SetFloatAsync(TParamId param, float value) {
			Log($"Setting float {param} to {value}.");
			if (floats.ContainsKey(param) && floats[param] == value) {
				Log($"Float {param} already equals {value}.");
				return;
			}
			floats[param] = value;
			await EvaluateTransitionsAsync(param);
		}

		/// <summary>
		/// Sets an int parameter value
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <param name="value">Value</param>
		public async Task SetIntAsync(TParamId param, int value) {
			Log($"Setting int {param} to {value}.");
			if (ints.ContainsKey(param) && ints[param] == value) {
				Log($"Int {param} already equals {value}.");
				return;
			}
			ints[param] = value;
			await EvaluateTransitionsAsync(param);
		}

		/// <summary>
		/// Sets a string parameter value
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <param name="value">Value</param>
		public async Task SetStringAsync(TParamId param, string value) {
			Log($"Setting string {param} to {value}.");
			if (strings.ContainsKey(param) && strings[param] == value) {
				Log($"String {param} already equals {value}.");
				return;
			}
			strings[param] = value;
			await EvaluateTransitionsAsync(param);
		}

		/// <summary>
		/// Sets a trigger parameter
		/// </summary>
		/// <param name="param">Parameter id</param>
		public async Task SetTriggerAsync(TParamId param) {
			Log($"Setting trigger {param}.");
			if (triggers.Contains(param)) {
				Log($"Trigger {param} already triggered.");
				return;
			}
			triggers.Add(param);
			await EvaluateTransitionsAsync(param);
		}

		#endregion

		#region Utility/Helpers


		public async Task SendMessageAsync(TMessageId message, object arg = null) {
			if (ActiveState != null) {
				await ActiveState.SendMessageAsync(this, message, arg);
			}
		}

		private async Task EvaluateTransitionsAsync(TParamId param) {
			if (ActiveState is StateMachine<TStateId, TParamId, TMessageId> substate) {
				await substate.EvaluateTransitionsAsync();
				return;
			}
			if (!canEvaluateTransitions) {
				Log($"Cannot transition: " + (
					!IsRunning ? "State machine is not running." :
					transitionLock.IsLocked ? "Transitions are locked." : "No active states."
				));
				return;
			}
			Log($"Evaluating transitions for {param}");
			if (activeTransitions.Any(x => x.HasParam(param))) {
				Log("The active state does not take this param into consideration. No need to evaluate transitions.");
				return;
			}
			await EvaluateTransitionsAsync();
		}

		private async Task EvaluateTransitionsAsync() {
			if (ActiveState is StateMachine<TStateId, TParamId, TMessageId> substate) {
				await substate.EvaluateTransitionsAsync();
				return;
			}
			if (!canEvaluateTransitions) {
				Log($"Cannot transition: " + (
					!IsRunning ? "State machine is not running." :
					transitionLock.IsLocked ? "Transitions are locked." : "No active states."
				));
				return;
			}
			Log($"Evaluating transitions.");
			foreach (Transition<TStateId, TParamId, TMessageId> transition in activeTransitions) {
				bool success = transition.EvaluateTransition(this);
				if (success) {
					Log("Valid transition found.");
					ResetTriggers();
					await transition.DoTransitionAsync(this);
					return;
				}
			}
			Log("No valid transition found.");
		}

		#endregion
	}
}
