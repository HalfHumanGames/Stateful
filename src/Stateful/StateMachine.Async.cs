#if TASKS

#if NETSTANDARD1_0
using Stateful.Utilities;
#endif
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Stateful {

	public partial class StateMachine<TStateId, TParamId, TMessageId> : State<TStateId, TParamId, TMessageId> {

		#region Delegate definitions

		public delegate Task OnTransitionAsyncHandler(
			StateMachine<TStateId, TParamId, TMessageId> stateMachine, 
			State<TStateId, TParamId, TMessageId> state
		);

		public delegate Task OnMessageAsyncHandler(
			StateMachine<TStateId, TParamId, TMessageId> stateMachine, 
			State<TStateId, TParamId, TMessageId> state, 
			object arg
		);

		public delegate Task<T> OnMessageAsyncHandler<T>(
			StateMachine<TStateId, TParamId, TMessageId> stateMachine, 
			State<TStateId, TParamId, TMessageId> state, 
			object arg
		);

		#endregion

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

		internal override async Task SendMessageAsync(
			StateMachine<TStateId, TParamId, TMessageId> stateMachine, TMessageId message, object arg
		) {
			await base.SendMessageAsync(stateMachine, message, arg);
			await SendMessageAsync(message, arg);
		}

		#endregion

		#region Flow control

		/// <summary>
		/// Stops the state machine using the state with the specified state id
		/// </summary>
		/// <param name="state">The state id of the state to start at</param>
		public async Task StartAsync(TStateId state) {
			if (IsRunning) {
				throw new InvalidOperationException("State machine is already running.");
			}
			if (!states.ContainsKey(state)) {
				throw new ArgumentException($"State {state} not found.");
			}
			Log("Starting state machine.");
			IsRunning = true;
			observables.ForEach(x => x.Changed += OnObservableChanged);
			await GoToAsync(state);
		}

		/// <summary>
		/// Starts the state machine using the first state added during configuration
		/// </summary>
		public async Task StartAsync() => await StartAsync(initialStateId);

		/// <summary>
		/// Stops the state machine
		/// </summary>
		public async Task StopAsync() {
			if (!IsRunning) {
				throw new InvalidOperationException("State machine is not running.");
			}
			Log("Stopping state machine.");
			if (activeStates.Count > 0) {
				await Task.WhenAll(states.Select(x => x.Value.ExitAsync(this)));
			}
			observables.ForEach(x => x.Changed -= OnObservableChanged);
			IsRunning = false;
			Reconfigure();
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
							return;
						}
					} while (parent.IsSubstate);
				}
				if (ActiveState is StateMachine<TStateId, TParamId, TMessageId>) {
					Log($"State {state} not found, searching children for state.");
					StateMachine<TStateId, TParamId, TMessageId> child;
					do {
						child = ActiveState as StateMachine<TStateId, TParamId, TMessageId>;
						if (child.states.ContainsKey(state)) {
							await child.GoToAsync(state);
							return;
						}
					} while (child.ActiveState is StateMachine<TStateId, TParamId, TMessageId>);
				}
				throw new ArgumentException($"State {state} not found.");
			}
			transitionLock.AddLock();
			if (activeStates.Count > 0) {
				await states[activeStates.Pop()].ExitAsync(this);
			}
			activeStates.Push(state);
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
				if (IsSubstate) {
					Log($"State {state} not found, searching parents for state.");
					StateMachine<TStateId, TParamId, TMessageId> parent = this;
					do {
						parent = parent.ParentState;
						if (parent.states.ContainsKey(state)) {
							await parent.PushAsync(state);
							return;
						}
					} while (parent.IsSubstate);
				}
				if (ActiveState is StateMachine<TStateId, TParamId, TMessageId>) {
					Log($"State {state} not found, searching children for state.");
					StateMachine<TStateId, TParamId, TMessageId> child;
					do {
						child = ActiveState as StateMachine<TStateId, TParamId, TMessageId>;
						if (child.states.ContainsKey(state)) {
							await child.PushAsync(state);
							return;
						}
					} while (child.ActiveState is StateMachine<TStateId, TParamId, TMessageId>);
				}
				throw new ArgumentException($"State {state} not found.");
			}
			transitionLock.AddLock();
			if (activeStates.Count > 0) {
				await states[activeStates.Peek()].PauseAsync(this);
			}
			activeStates.Push(state);
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
				await states[activeStates.Pop()].ExitAsync(this);
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
			if (!IsRunning) {
				throw new InvalidOperationException("Cannot set param when the state machine is not running.");
			}
			Log($"Setting bool {param} to {value}.");
			if (bools.ContainsKey(param) && bools[param] == value) {
				Log($"Bool {param} already equals {value}.");
				return;
			}
			bools[param] = value;
			await EvaluateTransitionsAsync(param.GetHashCode());
		}

		/// <summary>
		/// Sets an float parameter value
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <param name="value">Value</param>
		public async Task SetFloatAsync(TParamId param, float value, bool relative = false) {
			if (!IsRunning) {
				throw new InvalidOperationException("Cannot set param when the state machine is not running.");
			}
			if (relative) {
				Log($"Adding {value} to float {param}.");
				if (!floats.ContainsKey(param)) {
					floats.Add(param, value);
					return;
				}
				floats[param] += value;
			} else {
				Log($"Setting float {param} to {value}.");
				if (floats.ContainsKey(param) && floats[param] == value) {
					Log($"Float {param} already equals {value}.");
					return;
				}
				floats[param] = value;
			}
			await EvaluateTransitionsAsync(param.GetHashCode());
		}

		/// <summary>
		/// Sets an int parameter value
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <param name="value">Value</param>
		public async Task SetIntAsync(TParamId param, int value, bool relative = false) {
			if (!IsRunning) {
				throw new InvalidOperationException("Cannot set param when the state machine is not running.");
			}
			if (relative) {
				Log($"Adding {value} to int {param}.");
				if (!ints.ContainsKey(param)) {
					ints.Add(param, value);
					return;
				}
				ints[param] += value;
			} else {
				Log($"Setting int {param} to {value}.");
				if (ints.ContainsKey(param) && ints[param] == value) {
					Log($"Int {param} already equals {value}.");
					return;
				}
				ints[param] = value;
			}
			await EvaluateTransitionsAsync(param.GetHashCode());
		}

		/// <summary>
		/// Sets a string parameter value
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <param name="value">Value</param>
		public async Task SetStringAsync(TParamId param, string value, bool append = false) {
			if (!IsRunning) {
				throw new InvalidOperationException("Cannot set param when the state machine is not running.");
			}
			if (append) {
				Log($"Appending {value} to string {param}.");
				if (!strings.ContainsKey(param)) {
					strings.Add(param, value);
					return;
				}
				strings[param] += value;
			} else {
				Log($"Setting string {param} to {value}.");
				if (strings.ContainsKey(param) && strings[param] == value) {
					Log($"String {param} already equals {value}.");
					return;
				}
				strings[param] = value;
			}
			await EvaluateTransitionsAsync(param.GetHashCode());
		}

		/// <summary>
		/// Sets a trigger parameter
		/// </summary>
		/// <param name="param">Parameter id</param>
		public async Task SetTriggerAsync(TParamId param) {
			if (!IsRunning) {
				throw new InvalidOperationException("Cannot set param when the state machine is not running.");
			}
			Log($"Setting trigger {param}.");
			if (triggers.Contains(param)) {
				Log($"Trigger {param} already triggered.");
				return;
			}
			triggers.Add(param);
			await EvaluateTransitionsAsync(param.GetHashCode());
			UnsetTrigger(param);
		}

		#endregion

		#region Utility/Helpers

		public async Task SendMessageAsync(TMessageId message, object arg = null) {
			if (ActiveState == null) {
				throw new InvalidOperationException("No active state to send message to.");
			}
			await ActiveState.SendMessageAsync(this, message, arg);
		}

		public async Task<T> SendMessageAsync<T>(TMessageId message, object arg = null) {
			if (ActiveState == null) {
				throw new InvalidOperationException("No active state to send message to.");
			}
			return await ActiveState.SendMessageAsync<T>(this, message, arg);
		}

		private async Task EvaluateTransitionsAsync(int paramHashCode) {
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
			if (activeTransitions.Any(x => x.HasParam(paramHashCode))) {
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
					await transition.DoTransitionAsync(this);
					return;
				}
			}
			Log("No valid transition found.");
		}

		#endregion
	}
}

#endif
