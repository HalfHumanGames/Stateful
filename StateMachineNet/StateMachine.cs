using System;
using System.Collections.Generic;
using System.Linq;
using StateMachineNet.Utilities;

namespace StateMachineNet {

	public class StateMachine : StateMachine<string, string> { }
	public class StateMachine<TStateId> : StateMachine<TStateId, string> { }
	public class StateMachine<TStateId, TParamId> : State<TStateId, TParamId> {

		/// <summary>
		/// Creates a new builder for this state machine
		/// </summary>
		public IStateMachineBuilder<TStateId, TParamId> Builder => StateMachineBuilder.Create<TStateId, TParamId>();

		/// <summary>
		/// Checks if this state machine is a substate machine
		/// </summary>
		public bool IsSubstate => ParentState != null;

		/// <summary>
		/// Gets the parent state if this is a substate machine
		/// </summary>
		public StateMachine<TStateId, TParamId> ParentState { get; private set; }

		/// <summary>
		/// Enables or disables logging the state machine flow
		/// </summary>
		public Ref<bool> LogFlow = new Ref<bool>(false);

		/// <summary>
		/// Checks if the state machine is running
		/// </summary>
		public bool IsRunning { get; private set; }

		/// <summary>
		/// Gets the active state as a specified type
		/// </summary>
		/// <typeparam name="T">The state type</typeparam>
		/// <returns>Returns the active state as the specified type</returns>
		public T GetActiveState<T>() where T : State<TStateId, TParamId> => (T) ActiveState;

		/// <summary>
		/// Gets the active state
		/// </summary>
		public State<TStateId, TParamId> ActiveState => activeStates.Count > 0 ? states[ActiveStateId] : null;

		/// <summary>
		/// Gets the state id of the  specified state
		/// </summary>
		public TStateId ActiveStateId => activeStates.Peek();

		private TStateId initialStateId;
		private bool hasSetInitialStateId;
		private Lock transitionLock = new Lock();
		private Stack<TStateId> activeStates = new Stack<TStateId>();
		private Dictionary<TStateId, State<TStateId, TParamId>> states = new Dictionary<TStateId, State<TStateId, TParamId>>();
		private Dictionary<TStateId, List<Transition<TStateId, TParamId>>> transitions = new Dictionary<TStateId, List<Transition<TStateId, TParamId>>>();
		private List<Transition<TStateId, TParamId>> globalTransitions = new List<Transition<TStateId, TParamId>>();
		private Dictionary<TParamId, bool> bools = new Dictionary<TParamId, bool>();
		private Dictionary<TParamId, float> floats = new Dictionary<TParamId, float>();
		private Dictionary<TParamId, int> ints = new Dictionary<TParamId, int>();
		private Dictionary<TParamId, string> strings = new Dictionary<TParamId, string>();
		private HashSet<TParamId> triggers = new HashSet<TParamId>();

		#region Creation methods

		protected void Configure(IAddTransitionAddStateBuild<TStateId, TParamId> builder) => Configure(builder.Build);
		protected void Configure(StateMachine<TStateId, TParamId> config) {
			LogFlow = config.LogFlow;
			IsRunning = config.IsRunning;
			initialStateId = config.initialStateId;
			hasSetInitialStateId = config.hasSetInitialStateId;
			activeStates = config.activeStates;
			states = config.states;
			globalTransitions = config.globalTransitions;
			transitions = config.transitions;
			bools = config.bools;
			floats = config.floats;
			ints = config.ints;
			strings = config.strings;
			triggers = config.triggers;
		}

		#endregion

		#region State method overrides

		internal override void Enter(StateMachine<TStateId, TParamId> stateMachine) {
			if (IsSubstate) {
				StateMachine<TStateId, TParamId> parent = this;
				do {
					parent = parent.ParentState;
					globalTransitions.AddRange(parent.globalTransitions);
				} while (parent.IsSubstate);
			}
			base.Enter(stateMachine);
			Start();
		}

		internal override void Exit(StateMachine<TStateId, TParamId> stateMachine) {
			Stop();
			base.Exit(stateMachine);
		}

		#endregion

		#region Flow control

		/// <summary>
		/// Starts the state machine using the first state added during configuration
		/// </summary>
		public void Start() {
			Log("Starting state machine.");
			IsRunning = true;
			GoTo(initialStateId);
		}

		/// <summary>
		/// Stops the state machine using the state with the specified state id
		/// </summary>
		/// <param name="state">The state id of the state to start at</param>
		public void Start(TStateId state) {
			initialStateId = state;
			Start();
		}

		/// <summary>
		/// Stops the state machine
		/// </summary>
		public void Stop() {
			Log("Stopping state machine.");
			while (activeStates.Count > 0) {
				states[activeStates.Pop()].Exit(this);
			}
			IsRunning = false;
		}

		/// <summary>
		/// Transitions to the specified state irregardless of transitions and transition conditions
		/// </summary>
		/// <param name="state">The state id of the state to transition to</param>
		public void GoTo(TStateId state) {
			Log($"Going to state {state}.");
			if (!states.ContainsKey(state)) {
				if (IsSubstate) {
					Log($"State {state} not found, searching parents for state.");
					StateMachine<TStateId, TParamId> parent = this;
					do {
						parent = parent.ParentState;
						if (parent.states.ContainsKey(state)) {
							parent.GoTo(state);
							return;
						}
					} while (parent.IsSubstate);
				}
				throw new ArgumentException($"State {state} does not exist.");
			}
			transitionLock.AddLock();
			if (activeStates.Count > 0) {
				states[activeStates.Pop()].Exit(this);
			}
			activeStates.Push(state);
			states[activeStates.Peek()].Enter(this);
			transitionLock.RemoveLock();
			EvaluateTransitions();
		}

		/// <summary>
		/// Pushes the specified state onto the active states stack irregardless of transitions and transition conditions
		/// </summary>
		/// <param name="state">The state id of the state to push onto the active states stack</param>
		public void Push(TStateId state) {
			Log($"Pushing state {state}.");
			if (!states.ContainsKey(state)) {
				throw new ArgumentException($"State {state} does not exist.");
			}
			transitionLock.AddLock();
			if (activeStates.Count > 0) {
				states[activeStates.Peek()].Pause(this);
			}
			activeStates.Push(state);
			states[activeStates.Peek()].Enter(this);
			transitionLock.RemoveLock();
			EvaluateTransitions();
		}

		/// <summary>
		/// Pops the peek state of the active states stack irregardless of transitions and transition conditions
		/// </summary>
		public void Pop() {
			Log($"Popping state {ActiveStateId}.");
			transitionLock.AddLock();
			if (activeStates.Count > 0) {
				states[activeStates.Pop()].Exit(this);
			}
			if (activeStates.Count > 0) {
				states[activeStates.Peek()].Resume(this);
			}
			transitionLock.RemoveLock();
			EvaluateTransitions();
		}

		#endregion

		#region Set parameters

		/// <summary>
		/// Sets an bool parameter value
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <param name="value">Value</param>
		public void SetBool(TParamId param, bool value) {
			Log($"Setting bool {param} to {value}.");
			if (bools.ContainsKey(param) && bools[param] == value) {
				Log($"Bool {param} already equals {value}.");
				return;
			}
			bools[param] = value;
			EvaluateTransitions();
		}

		/// <summary>
		/// Sets an float parameter value
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <param name="value">Value</param>
		public void SetFloat(TParamId param, float value) {
			Log($"Setting float {param} to {value}.");
			if (floats.ContainsKey(param) && floats[param] == value) {
				Log($"Float {param} already equals {value}.");
				return;
			}
			floats[param] = value;
			EvaluateTransitions();
		}

		/// <summary>
		/// Sets an int parameter value
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <param name="value">Value</param>
		public void SetInt(TParamId param, int value) {
			Log($"Setting int {param} to {value}.");
			if (ints.ContainsKey(param) && ints[param] == value) {
				Log($"Int {param} already equals {value}.");
				return;
			}
			ints[param] = value;
			EvaluateTransitions();
		}

		/// <summary>
		/// Sets a string parameter value
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <param name="value">Value</param>
		public void SetString(TParamId param, string value) {
			Log($"Setting string {param} to {value}.");
			if (strings.ContainsKey(param) && strings[param] == value) {
				Log($"String {param} already equals {value}.");
				return;
			}
			strings[param] = value;
			EvaluateTransitions();
		}

		/// <summary>
		/// Sets a trigger parameter
		/// </summary>
		/// <param name="param">Parameter id</param>
		public void SetTrigger(TParamId param) {
			Log($"Setting trigger {param}.");
			if (triggers.Contains(param)) {
				Log($"Trigger {param} already triggered.");
				return;
			}
			triggers.Add(param);
			EvaluateTransitions();
		}

		private void ResetTriggers() {
			if (triggers.Count == 0) {
				return;
			}
			Log($"Retting triggers.");
			triggers.Clear();
		}

		#endregion

		#region Get parameters

		/// <summary>
		/// Gets a bool parameter value
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <returns>Value</returns>
		public bool GetBool(TParamId param) {
			if (!bools.ContainsKey(param)) {
				return false;
			}
			return bools[param];
		}

		/// <summary>
		/// Gets a float parameter value
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <returns>Value</returns>
		public float GetFloat(TParamId param) {
			if (!floats.ContainsKey(param)) {
				return 0;
			}
			return floats[param];
		}

		/// <summary>
		/// Gets an int parameter value
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <returns>Value</returns>
		public int GetInt(TParamId param) {
			if (!ints.ContainsKey(param)) {
				return 0;
			}
			return ints[param];
		}

		/// <summary>
		/// Gets a string parameter value
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <returns>Value</returns>
		public string GetString(TParamId param) {
			if (!strings.ContainsKey(param)) {
				return null;
			}
			return strings[param];
		}

		internal bool GetTrigger(TParamId param) => triggers.Contains(param);

		#endregion

		#region Utility/Helpers

		/// <summary>
		/// Casts a state machine from one type to another as long as they state the same base state machine class
		/// </summary>
		/// <typeparam name="TStateMachine">Type to cast to</typeparam>
		/// <returns>The casted state machine</returns>
		public TStateMachine As<TStateMachine>() where TStateMachine : StateMachine<TStateId, TParamId>, new() =>
			new TStateMachine() {
				LogFlow = LogFlow,
				IsRunning = IsRunning,
				initialStateId = initialStateId,
				hasSetInitialStateId = hasSetInitialStateId,
				activeStates = activeStates,
				states = states,
				globalTransitions = globalTransitions,
				transitions = transitions,
				bools = bools,
				floats = floats,
				ints = ints,
				strings = strings,
				triggers = triggers
			};

		private void EvaluateTransitions() {
			if (ActiveState is StateMachine<TStateId, TParamId> substate) {
				substate.EvaluateTransitions();
				return;
			}
			if (!IsRunning || transitionLock.IsLocked || activeStates.Count == 0) {
				return;
			}
			Log($"Evaluating transitions.");
			foreach (Transition<TStateId, TParamId> transition in transitions.ContainsKey(ActiveStateId) ?
				globalTransitions.Concat(transitions[ActiveStateId]) : globalTransitions
			) {
				bool success = transition.EvaluateTransition(this);
				if (success) {
					Log("Valid transition found.");
					ResetTriggers();
					transition.DoTransition(this);
					return;
				}
			}
			Log("No valid transition found.");
		}

		/// <summary>
		/// Gets a state by state id
		/// </summary>
		/// <param name="state">State id of state to get</param>
		/// <returns>State</returns>
		public State<TStateId, TParamId> GetState(TStateId state) => GetState<State<TStateId, TParamId>>(state);

		/// <summary>
		/// Gets a state by state id, but casted as the specified type
		/// </summary>
		/// <typeparam name="T">Type to cast the state as</typeparam>
		/// <param name="state">State id of state to get</param>
		/// <returns>State casted as the specifed type</returns>
		public T GetState<T>(TStateId state) where T : State<TStateId, TParamId> {
			if (!states.ContainsKey(state)) {
				throw new ArgumentException($"State {state} does not exist.");
			}
			return (T) states[state];
		}

		private void Log(string message) {
			if (!LogFlow.Value) {
				return;
			}
			Print.Info(message);
		}

		#endregion

		#region Internal methods used by StateMachineBuilder

		internal void AddState(TStateId stateId, State<TStateId, TParamId> state) {
			if (states.ContainsKey(stateId)) {
				throw new ArgumentException($"A state with the name \"{stateId}\" already exists.");
			}
			states[stateId] = state;
			if (!hasSetInitialStateId) {
				hasSetInitialStateId = true;
				initialStateId = stateId;
			}
			if (state is StateMachine<TStateId, TParamId> substate) {
				substate.ParentState = this;
				transitions = transitions.Union(substate.transitions).ToDictionary(x => x.Key, x => x.Value);
				bools = bools.Union(substate.bools).ToDictionary(x => x.Key, x => x.Value);
				floats = floats.Union(substate.floats).ToDictionary(x => x.Key, x => x.Value);
				ints = ints.Union(substate.ints).ToDictionary(x => x.Key, x => x.Value);
				strings = strings.Union(substate.strings).ToDictionary(x => x.Key, x => x.Value);
				triggers = triggers.Union(substate.triggers).ToHashSet<TParamId>();
				substate.LogFlow = LogFlow;
				substate.transitions = transitions;
				substate.bools = bools;
				substate.floats = floats;
				substate.ints = ints;
				substate.strings = strings;
				substate.triggers = triggers;
			}
		}

		internal void AddTransition(TStateId stateId, Transition<TStateId, TParamId> transition) {
			if (!transitions.ContainsKey(stateId)) {
				transitions[stateId] = new List<Transition<TStateId, TParamId>>();
			}
			transitions[stateId].Add(transition);
		}

		internal void AddGlobalTransition(Transition<TStateId, TParamId> transition) =>
			globalTransitions.Add(transition);

		#endregion
	}
}
