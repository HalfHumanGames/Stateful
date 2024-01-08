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

	public partial  class StateMachine<TStateId, TParamId, TMessageId> : State<TStateId, TParamId, TMessageId> {

		#region Delegate definitions

		public delegate void OnTransitionHandler(StateMachine<TStateId, TParamId, TMessageId> stateMachine, State<TStateId, TParamId, TMessageId> state);
		public delegate void OnMessageHandler(StateMachine<TStateId, TParamId, TMessageId> stateMachine, State<TStateId, TParamId, TMessageId> state, object arg);
		public delegate T OnMessageHandler<T>(StateMachine<TStateId, TParamId, TMessageId> stateMachine, State<TStateId, TParamId, TMessageId> state, object arg);

		#endregion

		#region Public variables

		public IAddStateAddSetParam<TStateId, TParamId, TMessageId> Builder => StateMachineBuilder.Create<TStateId, TParamId, TMessageId>();
		public bool IsRunning { get; protected set; }
		
		public bool LogFlow { get => IsSubstate ? ParentState.LogFlow : logFlow; set => logFlow = value;}
		public StateMachine<TStateId, TParamId, TMessageId> ParentState { get; private set; }
		public bool IsSubstate => ParentState != null;
		public int ActiveStatesCount => activeStates.Count;
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
		public State<TStateId, TParamId, TMessageId> ActiveState => activeStates.Count > 0 ? states[ActiveStateId] : null;
		public TStateId[] StateIds => states?.Keys.ToArray();

		#endregion

		#region Private variables

		private bool logFlow;

		// Default configuration
		private StateMachine<TStateId, TParamId, TMessageId> configuration;

		// State management
		private TStateId initialStateId;
		private bool hasSetInitialStateId;
		private Stack<TStateId> activeStates = new Stack<TStateId>();
		private Dictionary<TStateId, State<TStateId, TParamId, TMessageId>> states =
			new Dictionary<TStateId, State<TStateId, TParamId, TMessageId>>();

		// Transition management
		private bool isTransitioning;
		private Dictionary<TStateId, List<Transition<TStateId, TParamId, TMessageId>>> transitions =
			new Dictionary<TStateId, List<Transition<TStateId, TParamId, TMessageId>>>();
		private List<Transition<TStateId, TParamId, TMessageId>> activeTransitions => transitions.ContainsKey(ActiveStateId) ? transitions[ActiveStateId] : null;

		// Parameter management
		private Dictionary<TParamId, bool> bools = new Dictionary<TParamId, bool>();
		private Dictionary<TParamId, float> floats = new Dictionary<TParamId, float>();
		private Dictionary<TParamId, int> ints = new Dictionary<TParamId, int>();
		private Dictionary<TParamId, string> strings = new Dictionary<TParamId, string>();
		private HashSet<TParamId> triggers = new HashSet<TParamId>();
		private List<INotifyChanged> observables = new List<INotifyChanged>();

		#endregion

		#region Configure methods

		protected virtual IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> GetConfiguration() { 
			return Builder;
		}

		private void Configure(IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> builder) {
			if (builder == null) {
				throw new ArgumentException("Cannot configure null.");
			}
			configuration = builder.Build() ?? throw new ArgumentException("Cannot configure null.");
			Copy(configuration);
		}

		private void Reconfigure() {
			Copy(configuration);
		}

		#endregion

		#region Flow control

		public void Start(TStateId state) {
			if (IsRunning) {
				throw new InvalidOperationException("State machine is already running.");
			}
			if (configuration == null) {
				Configure(GetConfiguration());
			}
			if (!states.ContainsKey(state)) {
				throw new ArgumentException($"State {state} not found.");
			}
			Log("Starting state machine.");
			IsRunning = true;
			observables.ForEach(x => x.Changed += OnObservableChanged);
			GoTo(state);
		}

		public void Start() {
			Start(initialStateId);
		}

		private void OnObservableChanged(INotifyChanged observable) {
			int hashCode = observable.GetHashCode();
			EvaluateTransitions(hashCode);
		}

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
			if (activeStates.Count > 0) {
				states[activeStates.Pop()].Exit(this);
			}
			activeStates.Push(state);
			states[state].Enter(this);
			EvaluateTransitions();
		}

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
			if (activeStates.Count > 0) {
				states[activeStates.Peek()].Pause(this);
			}
			activeStates.Push(state);
			states[activeStates.Peek()].Enter(this);
			EvaluateTransitions();
		}

		public void Pop() {
			if (!IsRunning) {
				throw new InvalidOperationException("Cannot transition when the state machine is not running.");
			}
			Log($"Popping state {ActiveStateId}.");
			if (activeStates.Count == 0) {
				throw new InvalidOperationException("No active states to pop.");
			}
			states[activeStates.Pop()].Exit(this);
			if (activeStates.Count > 0) {
				states[activeStates.Peek()].Resume(this);
			}
			EvaluateTransitions();
		}

		#endregion

		#region Set parameters

		public void SetBool(TParamId param, bool value) {	
			Log($"Setting bool {param} to {value}.");
			if (bools.ContainsKey(param) && bools[param] == value) {
				Log($"Bool {param} already equals {value}.");
				return;
			}
			bools[param] = value;
			EvaluateTransitions(param.GetHashCode());
		}

		public void SetFloat(TParamId param, float value, bool relative = false) {
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

		public void SetInt(TParamId param, int value, bool relative = false) {
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

		public void SetString(TParamId param, string value, bool append = false) {
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

		public void SetTrigger(TParamId param) {
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

		public T GetActiveState<T>() where T : State<TStateId, TParamId, TMessageId> {
			return (T) ActiveState;
		}

		public State<TStateId, TParamId, TMessageId> GetState(TStateId state) {
			return GetState<State<TStateId, TParamId, TMessageId>>(state);
		}

		public T GetState<T>(TStateId state) where T : State<TStateId, TParamId, TMessageId> {
			if (!states.ContainsKey(state)) {
				throw new ArgumentException($"State {state} not found.");
			}
			return (T) states[state];
		}

		#endregion

		#region Parameter getters

		public bool GetBool(TParamId param) {
			return !bools.ContainsKey(param) ? false : bools[param];
		}

		public float GetFloat(TParamId param) {
			return !floats.ContainsKey(param) ? 0 : floats[param];
		}

		public int GetInt(TParamId param) {
			return !ints.ContainsKey(param) ? 0 : ints[param];
		}

		public string GetString(TParamId param) {
			return !strings.ContainsKey(param) ? null : strings[param];
		}

		// Used by transitions
		internal bool GetTrigger(TParamId param) {
			return triggers.Contains(param);
		}

		#endregion

		#region Public misc.

		public void SendMessage(TMessageId message, object arg = null) {
			if (!IsRunning) {
				throw new InvalidOperationException("Cannot send message when the state machine is not running.");
			}
			if (ActiveState == null) {
				throw new InvalidOperationException("No active state to send message to.");
			}
			ActiveState.SendMessage(this, message, arg);
		}

		public T SendMessage<T>(TMessageId message, object arg = null) {
			if (!IsRunning) {
				throw new InvalidOperationException("Cannot send message when the state machine is not running.");
			}
			if (ActiveState == null) {
				throw new InvalidOperationException("No active state to send message to.");
			}
			return ActiveState.SendMessage<T>(this, message, arg);
		}

		public TStateMachine As<TStateMachine>(params object[] args) where TStateMachine : StateMachine<TStateId, TParamId, TMessageId>, new() {
			if (IsRunning) {
				throw new InvalidOperationException("Only use As during configuration");
			}
			TStateMachine copy = (TStateMachine) Activator.CreateInstance(typeof(TStateMachine), args);
			copy.Copy(this);
			return copy;
		}

		#endregion

		#region Private utilities

		private void Copy(StateMachine<TStateId, TParamId, TMessageId> other) {
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

		private void EvaluateTransitions(int paramHashCode = 0) {
			if (ActiveState is StateMachine<TStateId, TParamId, TMessageId> substate) {
				substate.EvaluateTransitions();
				return;
			}
			if (!CanEvaluateTransitions(out string reason)) {
				Log($"Cannot transition: " + reason);
				return;
			}
			if (paramHashCode != 0 && !activeTransitions.Any(x => x.HasParam(paramHashCode))) {
				Log("The active state does not take this param into consideration. No need to evaluate transitions.");
				return;
			}
			Log($"Evaluating transitions.");
			foreach (Transition<TStateId, TParamId, TMessageId> transition in activeTransitions) {
				bool success = transition.EvaluateTransition(this);
				if (success) {
					Log("Valid transition found.");
					triggers.Clear();
					isTransitioning = true;
					transition.DoTransition(this);
					isTransitioning = false;
					return;
				}
			}
			Log("No valid transition found.");
		}

		private bool CanEvaluateTransitions(out string error) {
			if (isTransitioning) {
				error = "Already transitioning.";
				return false;
			}
			if (activeStates.Count == 0) {
				error = "No active states";
				return false;
			}
			if (activeTransitions == null || activeTransitions.Count == 0) {
				error = "No active transitions";
				return false;
			}
			error = "";
			return true;
		}

		private void Log(string message) {
			if (!LogFlow) {
				return;
			}
			Print.Info(message);
		}

		#endregion

		#region State method overrides

		internal override void Enter(StateMachine<TStateId, TParamId, TMessageId> stateMachine) {
			base.Enter(stateMachine);
			Start();
		}

		internal override void Exit(StateMachine<TStateId, TParamId, TMessageId> stateMachine) {
			Stop();
			base.Exit(stateMachine);
		}

		internal override void SendMessage(StateMachine<TStateId, TParamId, TMessageId> stateMachine, TMessageId message, object arg) {
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
				transitions = transitions.Union(substate.transitions).ToDictionary(x => x.Key, x => x.Value);
				bools = bools.Union(substate.bools).ToDictionary(x => x.Key, x => x.Value);
				floats = floats.Union(substate.floats).ToDictionary(x => x.Key, x => x.Value);
				ints = ints.Union(substate.ints).ToDictionary(x => x.Key, x => x.Value);
				strings = strings.Union(substate.strings).ToDictionary(x => x.Key, x => x.Value);
				// Linq ToHashSet unavailable in .NET Standard 2.0
				triggers = new HashSet<TParamId>(triggers.Union(substate.triggers));
				observables = observables.Union(substate.observables).ToList();
				substate.ParentState = this;
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

		internal void AddObservable<T>(INotifyChanged observable, Func<T, bool> check) {
			observables.Add(observable);
		}

		#endregion
	}
}
