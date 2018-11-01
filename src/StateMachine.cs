using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using StateMachineNet.Utilities;

namespace StateMachineNet {

	#region Subclasses

	[Serializable] public class StateMachine : StateMachine<string, string, string> { }
	[Serializable] public class StateMachine<TStateId> : StateMachine<TStateId, string, string> { }
	[Serializable] public class StateMachine<TStateId, TParamId> : StateMachine<TStateId, TParamId, string> { }

	#endregion

	[Serializable]
	public partial class StateMachine<TStateId, TParamId, TMessageId> : State<TStateId, TParamId, TMessageId> {

		#region Delegate definitions

		public delegate void OnTransitionHandler(
			StateMachine<TStateId, TParamId, TMessageId> stateMachine, 
			State<TStateId, TParamId, TMessageId> state
		);

		public delegate void OnMessageHandler(
			StateMachine<TStateId, TParamId, TMessageId> stateMachine, 
			State<TStateId, TParamId, TMessageId> state, 
			object arg
		);

		#endregion

		#region Public variables

		/// <summary>
		/// Creates a new builder for this state machine
		/// </summary>
		public IStateMachineBuilder<TStateId, TParamId, TMessageId> Builder => 
			StateMachineBuilder.Create<TStateId, TParamId, TMessageId>();

		/// <summary>
		/// Checks if this state machine is a substate machine
		/// </summary>
		public bool IsSubstate => ParentStateId != null;

		/// <summary>
		/// Gets the parent state if this is a substate machine
		/// </summary>
		public StateMachine<TStateId, TParamId, TMessageId> ParentStateId { get; private set; }

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
		public T GetActiveState<T>() where T : State<TStateId, TParamId, TMessageId> => (T) ActiveState;

		/// <summary>
		/// Gets the active state
		/// </summary>
		public State<TStateId, TParamId, TMessageId> ActiveState => activeStates.Count > 0 ? states[ActiveStateId] : null;

		/// <summary>
		/// Gets the state id of the active state
		/// </summary>
		public TStateId ActiveStateId => activeStates.Peek();

		#endregion

		#region Private variables

		// State management
		private TStateId initialStateId;
		private bool hasSetInitialStateId;
		private Stack<TStateId> activeStates = new Stack<TStateId>();
		private Dictionary<TStateId, State<TStateId, TParamId, TMessageId>> states = 
			new Dictionary<TStateId, State<TStateId, TParamId, TMessageId>>();

		// Transition management
		private Lock transitionLock = new Lock();
		private Dictionary<TStateId, List<Transition<TStateId, TParamId, TMessageId>>> transitions = 
			new Dictionary<TStateId, List<Transition<TStateId, TParamId, TMessageId>>>();
		private List<Transition<TStateId, TParamId, TMessageId>> globalTransitions = 
			new List<Transition<TStateId, TParamId, TMessageId>>();
		private bool canEvaluateTransitions => IsRunning && !transitionLock.IsLocked && activeStates.Count > 0;
		private List<Transition<TStateId, TParamId, TMessageId>> activeTransitions => transitions.ContainsKey(ActiveStateId) ?
			globalTransitions.Concat(transitions[ActiveStateId]).ToList() : globalTransitions;

		// Parameter management
		private Dictionary<TParamId, bool> bools = new Dictionary<TParamId, bool>();
		private Dictionary<TParamId, float> floats = new Dictionary<TParamId, float>();
		private Dictionary<TParamId, int> ints = new Dictionary<TParamId, int>();
		private Dictionary<TParamId, string> strings = new Dictionary<TParamId, string>();
		private HashSet<TParamId> triggers = new HashSet<TParamId>();
		private Dictionary<TStateId, List<Action>> observableSubscriptions = new Dictionary<TStateId, List<Action>>();
		private Dictionary<TStateId, List<Action>> observableUnsubscriptions = new Dictionary<TStateId, List<Action>>();

		#endregion

		#region Configure methods

		protected void Configure(IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> builder) => Configure(builder.Build);
		protected void Configure(StateMachine<TStateId, TParamId, TMessageId> config) {
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

		internal override void Enter(StateMachine<TStateId, TParamId, TMessageId> stateMachine) {
			if (IsSubstate) {
				StateMachine<TStateId, TParamId, TMessageId> parent = this;
				do {
					parent = parent.ParentStateId;
					globalTransitions.AddRange(parent.globalTransitions);
				} while (parent.IsSubstate);
			}
			base.Enter(stateMachine);
			Start();
		}

		internal override void Exit(StateMachine<TStateId, TParamId, TMessageId> stateMachine) {
			Stop();
			base.Exit(stateMachine);
		}

		internal override void SendMessage(
			StateMachine<TStateId, TParamId, TMessageId> stateMachine, TMessageId message, object arg
		) {
			base.SendMessage(stateMachine, message, arg);
			SendMessage(message, arg);
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
					StateMachine<TStateId, TParamId, TMessageId> parent = this;
					do {
						parent = parent.ParentStateId;
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
				TStateId popped = activeStates.Pop();
				UnsubscribeFromObservables(popped);
				states[popped].Exit(this);
			}
			activeStates.Push(state);
			SubscribeToObservables(state);
			states[state].Enter(this);
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
			SubscribeToObservables(state);
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
				TStateId popped = activeStates.Pop();
				UnsubscribeFromObservables(popped);
				states[popped].Exit(this);
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
			EvaluateTransitions(param);
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
			EvaluateTransitions(param);
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
			EvaluateTransitions(param);
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
			EvaluateTransitions(param);
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
			EvaluateTransitions(param);
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

		#region Public misc.

		/// <summary>
		/// Sends a message to the active state
		/// </summary>
		/// <param name="message">Message id</param>
		/// <param name="arg">Message data</param>
		public void SendMessage(TMessageId message, object arg = null) {
			if (ActiveState != null) {
				ActiveState.SendMessage(this, message, arg);
			}
		}

		/// <summary>
		/// Casts a state machine from one type to another as long as they share the same base state machine class
		/// </summary>
		/// <typeparam name="TStateIdMachine">Type to cast to</typeparam>
		/// <param name="args">Constructor arguments</param>
		/// <returns>The casted state machine</returns>
		public TStateIdMachine As<TStateIdMachine>(params object[] args) 
			where TStateIdMachine : StateMachine<TStateId, TParamId, TMessageId>, new() {

			TStateIdMachine copy = (TStateIdMachine) Activator.CreateInstance(typeof(TStateIdMachine), args);
			copy.LogFlow = LogFlow;
			copy.IsRunning = IsRunning;
			copy.initialStateId = initialStateId;
			copy.hasSetInitialStateId = hasSetInitialStateId;
			copy.activeStates = activeStates;
			copy.states = states;
			copy.globalTransitions = globalTransitions;
			copy.transitions = transitions;
			copy.bools = bools;
			copy.floats = floats;
			copy.ints = ints;
			copy.strings = strings;
			copy.triggers = triggers;
			return copy;
		}

		#endregion

		#region Private utilities

		private void EvaluateTransitions(TParamId param) {
			if (ActiveState is StateMachine<TStateId, TParamId, TMessageId> substate) {
				substate.EvaluateTransitions();
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
			if (!activeTransitions.Any(x => x.HasParam(param))) {
				Log("The active state does not take this param into consideration. No need to evaluate transitions.");
				return;
			}
			EvaluateTransitions();
		}

		private void EvaluateTransitions() {
			if (ActiveState is StateMachine<TStateId, TParamId, TMessageId> substate) {
				substate.EvaluateTransitions();
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
					transition.DoTransition(this);
					return;
				}
			}
			Log("No valid transition found.");
		}

		private void SubscribeToObservables(TStateId state) {
			if (!observableSubscriptions.ContainsKey(state) || !transitions.ContainsKey(state)) {
				return;
			}
			observableSubscriptions[state].ForEach(x => x());
			transitions[state].ForEach(x => {
				x.RefreshObservables();
				x.SubscribeToObservables();
			});
		}

		private void UnsubscribeFromObservables(TStateId state) {
			if (!observableUnsubscriptions.ContainsKey(state) || !transitions.ContainsKey(state)) {
				return;
			}
			observableUnsubscriptions[state].ForEach(x => x());
			transitions[state].ForEach(x => x.UnsubscribeFromObservables());
		}

		private void Log(string message) {
			if (!LogFlow.Value) {
				return;
			}
			Print.Info(message);
		}

		#endregion

		#region Get state

		/// <summary>
		/// Gets a state by state id
		/// </summary>
		/// <param name="state">State id of state to get</param>
		/// <returns>State</returns>
		public State<TStateId, TParamId, TMessageId> GeTStateId(TStateId state) => 
			GeTStateId<State<TStateId, TParamId, TMessageId>>(state);

		/// <summary>
		/// Gets a state by state id, but casted as the specified type
		/// </summary>
		/// <typeparam name="T">Type to cast the state as</typeparam>
		/// <param name="state">State id of state to get</param>
		/// <returns>State casted as the specifed type</returns>
		public T GeTStateId<T>(TStateId state) where T : State<TStateId, TParamId, TMessageId> {
			if (!states.ContainsKey(state)) {
				throw new ArgumentException($"State {state} does not exist.");
			}
			return (T) states[state];
		}

		#endregion

		#region Serialize/deserialize

		public byte[] Serialize() {
			BinaryFormatter formatter = new BinaryFormatter();
			using (MemoryStream stream = new MemoryStream()) {
				formatter.Serialize(stream, this);
				return stream.ToArray();
			}
		}

		public void Deserialize(byte[] data) {
			BinaryFormatter formatter = new BinaryFormatter();
			using (MemoryStream stream = new MemoryStream(data)) {
				object deserialized = formatter.Deserialize(stream);
				Configure((StateMachine<TStateId, TParamId, TMessageId>) deserialized);
			}
		}

		#endregion

		#region Internal methods used by StateMachineBuilder

		internal void AddState(TStateId stateId, State<TStateId, TParamId, TMessageId> state) {
			if (states.ContainsKey(stateId)) {
				throw new ArgumentException($"A state with the name \"{stateId}\" already exists.");
			}
			states[stateId] = state;
			if (!hasSetInitialStateId) {
				hasSetInitialStateId = true;
				initialStateId = stateId;
			}
			if (state is StateMachine<TStateId, TParamId, TMessageId> substate) {
				substate.ParentStateId = this;
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

		internal void AddTransition(TStateId stateId, Transition<TStateId, TParamId, TMessageId> transition) {
			if (!transitions.ContainsKey(stateId)) {
				transitions[stateId] = new List<Transition<TStateId, TParamId, TMessageId>>();
			}
			transitions[stateId].Add(transition);
		}

		internal void AddGlobalTransition(Transition<TStateId, TParamId, TMessageId> transition) =>
			globalTransitions.Add(transition);


		// CANNOT ACCESS STATE VIA DICT BEFORE IT GETS ADDED!!!!!
		internal void AddObservable<T>(TStateId[] states, Observable<T> observable) {
			for (int i = 0; i < states.Length; i++) {
				if (!observableSubscriptions.ContainsKey(states[i])) {
					observableSubscriptions.Add(states[i], new List<Action>());
				}
				observableSubscriptions[states[i]].Add(() => observable.ValueChanged += OnValueChanged);
				observableUnsubscriptions[states[i]].Add(() => observable.ValueChanged -= OnValueChanged);
			}
			void OnValueChanged(Observable<T> o, T p, T n) {
				Log("An observable value has changed from {p} to {n}.");
				EvaluateTransitions();
			}
		}

		#endregion
	}
}