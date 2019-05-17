using System;
using System.Collections.Generic;
using System.Linq;
using Stateful.Utilities;

namespace Stateful {

	#region Subclasses

	public class StateMachine : StateMachine<string, string, string> { }
	public class StateMachine<TStateId> : StateMachine<TStateId, string, string> { }
	public class StateMachine<TStateId, TParamId> : StateMachine<TStateId, TParamId, string> { }

	#endregion

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

		public delegate T OnMessageHandler<T>(
			StateMachine<TStateId, TParamId, TMessageId> stateMachine,
			State<TStateId, TParamId, TMessageId> state,
			object arg
		);

		#endregion

		#region Public variables

		/// <summary>
		/// Creates a new builder for this state machine
		/// </summary>
		public IAddStateAddSetParam<TStateId, TParamId, TMessageId> Builder =>
			StateMachineBuilder.Create<TStateId, TParamId, TMessageId>();

		/// <summary>
		/// Checks if the state machine is running
		/// </summary>
		public bool IsRunning {
			get;
			private set;
		}

		/// <summary>
		/// Enables or disables logging the state machine flow
		/// </summary>
		public bool LogFlow { get => logFlow.Value; set => logFlow.Value = value; }
		private Ref<bool> logFlow = new Ref<bool>(false);

		/// <summary>
		/// Gets the parent state if this is a substate machine or null if otherwise
		/// </summary>
		public StateMachine<TStateId, TParamId, TMessageId> ParentState { get; private set; }

		/// <summary>
		/// Checks if this state machine is a substate machine
		/// </summary>
		public bool IsSubstate => ParentState != null;

		/// <summary>
		/// Gets the number of active states
		/// </summary>
		public int ActiveStatesCount => activeStates.Count;

		/// <summary>
		/// Gets the state id of the active state
		/// </summary>
		public TStateId ActiveStateId {
			get {
				if (activeStates.Count == 0) {
					throw new InvalidOperationException(
						"No active states. Ensure ActiveStateCount > 0 before accessing the active state id."
					);
				}
				return activeStates.Peek();
			}
		}

		/// <summary>
		/// Gets the active state
		/// </summary>
		public State<TStateId, TParamId, TMessageId> ActiveState => activeStates.Count > 0 ? states[ActiveStateId] : null;

		public TStateId[] StateIds => states?.Keys.ToArray();

		#endregion

		#region Private variables

		// Default configuration
		private StateMachine<TStateId, TParamId, TMessageId> configuration;

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
		private List<Transition<TStateId, TParamId, TMessageId>> activeTransitions =>
			transitions.ContainsKey(ActiveStateId) ? transitions[ActiveStateId] : null;
		private bool canEvaluateTransitions => IsRunning && !transitionLock.IsLocked && activeStates.Count > 0;

		// Parameter management
		private Dictionary<TParamId, bool> bools = new Dictionary<TParamId, bool>();
		private Dictionary<TParamId, float> floats = new Dictionary<TParamId, float>();
		private Dictionary<TParamId, int> ints = new Dictionary<TParamId, int>();
		private Dictionary<TParamId, string> strings = new Dictionary<TParamId, string>();
		private HashSet<TParamId> triggers = new HashSet<TParamId>();
		private List<IObservable> observables = new List<IObservable>();

		#endregion

		#region Configure methods

		protected void Configure(IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> builder) {
			if (builder == null) {
				throw new ArgumentException("Cannot configure null.");
			}
			Configure(builder.Build);
		}

		protected void Configure(StateMachine<TStateId, TParamId, TMessageId> configuration) {
			if (configuration == null) {
				throw new ArgumentException("Cannot configure null.");
			}
			this.configuration = configuration;
			Copy(configuration);
		}

		private void Reconfigure() {
			Copy(configuration);
		}

		#endregion

		#region Flow control

		/// <summary>
		/// Stops the state machine using the state with the specified state id
		/// </summary>
		/// <param name="state">The state id of the state to start at</param>
		public void Start(TStateId state) {
			if (IsRunning) {
				throw new InvalidOperationException("State machine is already running.");
			}
			if (configuration == null) {
				Configure(this);
			}
			if (!states.ContainsKey(state)) {
				throw new ArgumentException($"State {state} not found.");
			}
			Log("Starting state machine.");
			IsRunning = true;
			observables.ForEach(x => x.Changed += OnObservableChanged);
			GoTo(state);
		}


		/// <summary>
		/// Starts the state machine using the first state added during configuration
		/// </summary>
		public void Start() {
			Start(initialStateId);
		}

		private void OnObservableChanged(IObservable observable) {
			int hashCode = observable.GetHashCode();
			EvaluateTransitions(hashCode);
		}

		/// <summary>
		/// Stops the state machine
		/// </summary>
		public void Stop() {
			if (!IsRunning) {
				throw new InvalidOperationException("State machine is not running.");
			}
			Log("Stopping state machine.");
			while (activeStates.Count > 0) {
				states[activeStates.Pop()].Exit(this);
			}
			observables.ForEach(x => x.Changed -= OnObservableChanged);
			IsRunning = false;
			Reconfigure();
		}

		/// <summary>
		/// Transitions to the specified state irregardless of transitions and transition conditions
		/// </summary>
		/// <param name="state">The state id of the state to transition to</param>
		public void GoTo(TStateId state) {
			if (!IsRunning) {
				throw new InvalidOperationException("Cannot transition when the state machine is not running.");
			}
			Log($"Going to state {state}.");
			if (!states.ContainsKey(state)) {
				if (IsSubstate) {
					Log($"State {state} not found, searching parents for state.");
					StateMachine<TStateId, TParamId, TMessageId> parent = this;
					do {
						parent = parent.ParentState;
						if (parent.states.ContainsKey(state)) {
							parent.GoTo(state);
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
							child.GoTo(state);
							return;
						}
					} while (child.ActiveState is StateMachine<TStateId, TParamId, TMessageId>);
				}
				throw new ArgumentException($"State {state} not found.");
			}
			transitionLock.AddLock();
			if (activeStates.Count > 0) {
				states[activeStates.Pop()].Exit(this);
			}
			activeStates.Push(state);
			states[state].Enter(this);
			transitionLock.RemoveLock();
			EvaluateTransitions();
		}

		/// <summary>
		/// Pushes the specified state onto the active states stack irregardless of transitions and transition conditions
		/// </summary>
		/// <param name="state">The state id of the state to push onto the active states stack</param>
		public void Push(TStateId state) {
			if (!IsRunning) {
				throw new InvalidOperationException("Cannot transition when the state machine is not running.");
			}
			Log($"Pushing state {state}.");
			if (activeStates.Contains(state)) {
				throw new InvalidOperationException($"State {state} is already active.");
			}
			if (!states.ContainsKey(state)) {
				if (IsSubstate) {
					Log($"State {state} not found, searching parents for state.");
					StateMachine<TStateId, TParamId, TMessageId> parent = this;
					do {
						parent = parent.ParentState;
						if (parent.states.ContainsKey(state)) {
							parent.Push(state);
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
							child.Push(state);
							return;
						}
					} while (child.ActiveState is StateMachine<TStateId, TParamId, TMessageId>);
				}
				throw new ArgumentException($"State {state} not found.");
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
			if (!IsRunning) {
				throw new InvalidOperationException("Cannot transition when the state machine is not running.");
			}
			Log($"Popping state {ActiveStateId}.");
			if (activeStates.Count == 0) {
				throw new InvalidOperationException("No active states to pop.");
			}
			transitionLock.AddLock();
			states[activeStates.Pop()].Exit(this);
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
			if (!IsRunning) {
				throw new InvalidOperationException("Cannot set param when the state machine is not running.");
			}
			Log($"Setting bool {param} to {value}.");
			if (bools.ContainsKey(param) && bools[param] == value) {
				Log($"Bool {param} already equals {value}.");
				return;
			}
			bools[param] = value;
			EvaluateTransitions(param.GetHashCode());
		}

		/// <summary>
		/// Sets an float parameter value
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <param name="value">Value</param>
		public void SetFloat(TParamId param, float value, bool relative = false) {
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
			EvaluateTransitions(param.GetHashCode());
		}

		/// <summary>
		/// Sets an int parameter value
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <param name="value">Value</param>
		public void SetInt(TParamId param, int value, bool relative = false) {
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
			EvaluateTransitions(param.GetHashCode());
		}

		/// <summary>
		/// Sets a string parameter value
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <param name="value">Value</param>
		public void SetString(TParamId param, string value, bool append = false) {
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
			EvaluateTransitions(param.GetHashCode());
		}

		/// <summary>
		/// Sets a trigger parameter
		/// </summary>
		/// <param name="param">Parameter id</param>
		public void SetTrigger(TParamId param) {
			if (!IsRunning) {
				throw new InvalidOperationException("Cannot set param when the state machine is not running.");
			}
			Log($"Setting trigger {param}.");
			if (triggers.Contains(param)) {
				Log($"Trigger {param} already triggered.");
				return;
			}
			triggers.Add(param);
			EvaluateTransitions(param.GetHashCode());
			UnsetTrigger(param);
		}

		#endregion

		#region State getters

		/// <summary>
		/// Gets the active state as a specified type
		/// </summary>
		/// <typeparam name="T">The state type</typeparam>
		/// <returns>Returns the active state as the specified type</returns>
		public T GetActiveState<T>() where T : State<TStateId, TParamId, TMessageId> {
			return (T) ActiveState;
		}

		/// <summary>
		/// Gets a state by state id
		/// </summary>
		/// <param name="state">State id of state to get</param>
		/// <returns>State</returns>
		public State<TStateId, TParamId, TMessageId> GetState(TStateId state) {
			return GetState<State<TStateId, TParamId, TMessageId>>(state);
		}

		/// <summary>
		/// Gets a state by state id, but casted as the specified type
		/// </summary>
		/// <typeparam name="T">Type to cast the state as</typeparam>
		/// <param name="state">State id of state to get</param>
		/// <returns>State casted as the specifed type</returns>
		public T GetState<T>(TStateId state) where T : State<TStateId, TParamId, TMessageId> {
			if (!states.ContainsKey(state)) {
				throw new ArgumentException($"State {state} not found.");
			}
			return (T) states[state];
		}

		#endregion

		#region Parameter getters

		/// <summary>
		/// Gets a bool parameter value
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <returns>Value</returns>
		public bool GetBool(TParamId param) {
			return !bools.ContainsKey(param) ? false : bools[param];
		}

		/// <summary>
		/// Gets a float parameter value
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <returns>Value</returns>
		public float GetFloat(TParamId param) {
			return !floats.ContainsKey(param) ? 0 : floats[param];
		}

		/// <summary>
		/// Gets an int parameter value
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <returns>Value</returns>
		public int GetInt(TParamId param) {
			return !ints.ContainsKey(param) ? 0 : ints[param];
		}

		/// <summary>
		/// Gets a string parameter value
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <returns>Value</returns>
		public string GetString(TParamId param) {
			return !strings.ContainsKey(param) ? null : strings[param];
		}

		// Used by transitions
		internal bool GetTrigger(TParamId param) {
			return triggers.Contains(param);
		}

		#endregion

		#region Public misc.

		/// <summary>
		/// Sends a message to the active state
		/// </summary>
		/// <param name="message">Message id</param>
		/// <param name="arg">Message data</param>
		public void SendMessage(TMessageId message, object arg = null) {
			if (!IsRunning) {
				throw new InvalidOperationException("Cannot send message when the state machine is not running.");
			}
			if (ActiveState == null) {
				throw new InvalidOperationException("No active state to send message to.");
			}
			ActiveState.SendMessage(this, message, arg);
		}

		/// <summary>
		/// Sends a message to the active state
		/// </summary>
		/// <param name="message">Message id</param>
		/// <param name="arg">Message data</param>
		public T SendMessage<T>(TMessageId message, object arg = null) {
			if (!IsRunning) {
				throw new InvalidOperationException("Cannot send message when the state machine is not running.");
			}
			if (ActiveState == null) {
				throw new InvalidOperationException("No active state to send message to.");
			}
			return ActiveState.SendMessage<T>(this, message, arg);
		}

		/// <summary>
		/// Casts a state machine from one type to another as long as they share the same base state machine class
		/// </summary>
		/// <typeparam name="TStateMachine">Type to cast to</typeparam>
		/// <param name="args">Constructor arguments</param>
		/// <returns>The casted state machine</returns>
		public TStateMachine As<TStateMachine>(params object[] args)
			where TStateMachine : StateMachine<TStateId, TParamId, TMessageId>, new() {

			if (IsRunning) {
				throw new InvalidOperationException("Only use As during configuration");
			}
			TStateMachine copy = (TStateMachine) Activator.CreateInstance(typeof(TStateMachine), args);
			copy.Copy(this);
			return copy;
		}

		#endregion

		//#region Serialize/deserialize

		//public byte[] Serialize() {
		//	BinaryFormatter formatter = new BinaryFormatter();
		//	using (MemoryStream stream = new MemoryStream()) {
		//		formatter.Serialize(stream, this);
		//		return stream.ToArray();
		//	}
		//}

		//public void Deserialize(byte[] data) {
		//	BinaryFormatter formatter = new BinaryFormatter();
		//	using (MemoryStream stream = new MemoryStream(data)) {
		//		object deserialized = formatter.Deserialize(stream);
		//		Configure((StateMachine<TStateId, TParamId, TMessageId>) deserialized);
		//	}
		//}

		//#endregion

		#region Private utilities

		private void Copy(StateMachine<TStateId, TParamId, TMessageId> other) {
			logFlow.Value = other.logFlow.Value;
			IsRunning = other.IsRunning;
			initialStateId = other.initialStateId;
			hasSetInitialStateId = other.hasSetInitialStateId;
			states = other.states.ToDictionary(x => x.Key, x => x.Value);
			transitions = other.transitions.ToDictionary(x => x.Key, x => x.Value);
			bools = other.bools.ToDictionary(x => x.Key, x => x.Value);
			floats = other.floats.ToDictionary(x => x.Key, x => x.Value);
			ints = other.ints.ToDictionary(x => x.Key, x => x.Value);
			strings = other.strings.ToDictionary(x => x.Key, x => x.Value);
			// Linq ToHashSet unavailable in .NET Standard 2.0
			triggers = new HashSet<TParamId>(other.triggers);
			observables = other.observables.ToList();
		}

		private void UnsetTrigger(TParamId param) {
			if (triggers.Contains(param)) {
				Log($"Unsetting trigger {param}.");
				triggers.Remove(param);
				return;
			}
		}

		private void EvaluateTransitions(int paramHashCode) {
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
			if (!activeTransitions.Any(x => x.HasParam(paramHashCode))) {
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
					transition.DoTransition(this);
					return;
				}
			}
			Log("No valid transition found.");
		}

		private void Log(string message) {
			if (!logFlow.Value) {
				return;
			}
			Print.Info(message);
		}

		#endregion

		#region State method overrides

		internal override void Enter(StateMachine<TStateId, TParamId, TMessageId> stateMachine) {
			if (IsSubstate) {
				StateMachine<TStateId, TParamId, TMessageId> parent = this;
				do {
					parent = parent.ParentState;
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
				substate.ParentState = this;
				transitions = transitions.Union(substate.transitions).ToDictionary(x => x.Key, x => x.Value);
				bools = bools.Union(substate.bools).ToDictionary(x => x.Key, x => x.Value);
				floats = floats.Union(substate.floats).ToDictionary(x => x.Key, x => x.Value);
				ints = ints.Union(substate.ints).ToDictionary(x => x.Key, x => x.Value);
				strings = strings.Union(substate.strings).ToDictionary(x => x.Key, x => x.Value);
				// Linq ToHashSet unavailable in .NET Standard 2.0
				triggers = new HashSet<TParamId>(triggers.Union(substate.triggers));
				observables = observables.Union(substate.observables).ToList();
				substate.IsRunning = false;
				substate.logFlow = logFlow;
				substate.transitions = transitions;
				substate.bools = bools;
				substate.floats = floats;
				substate.ints = ints;
				substate.strings = strings;
				substate.triggers = triggers;
				substate.observables = observables;
			}
		}

		internal void AddTransition(TStateId stateId, Transition<TStateId, TParamId, TMessageId> transition) {
			if (!transitions.ContainsKey(stateId)) {
				transitions[stateId] = new List<Transition<TStateId, TParamId, TMessageId>>();
			}
			transitions[stateId].Add(transition);
		}

		internal void AddObservable<T>(Observable<T> observable, Func<T, bool> check) {
			observables.Add(observable);
		}

		#endregion
	}
}
