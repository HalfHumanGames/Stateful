using System;
using StateMachineNet.Utilities;

namespace StateMachineNet {

	// Implemented by StateMachineBuilder
	public interface IStateMachineBuilderFluentInterface<TStateId, TParamId, TMessageId> : IStateMachineBuilder<TStateId, TParamId, TMessageId>, IAddConditionAddHandlerAddTransitionAddStateBuildAddOr<TStateId, TParamId, TMessageId> { }

	// Initializer
	public interface IStateMachineBuilder<TStateId, TParamId, TMessageId> : IAddState<TStateId, TParamId, TMessageId> {
		
		/// <summary>
		/// Sets the value of a bool parameter
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <param name="value">Value</param>
		/// <returns>Returns a fluent interface</returns>
		IStateMachineBuilder<TStateId, TParamId, TMessageId> SetBool(TParamId param, bool value);
		
		/// <summary>
		/// Sets the value of a float parameter
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <param name="value">Value</param>
		/// <returns>Returns a fluent interface</returns>
		IStateMachineBuilder<TStateId, TParamId, TMessageId> SetFloat(TParamId param, float value);
		
		/// <summary>
		/// Sets the value of an int parameter
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <param name="value">Value</param>
		/// <returns>Returns a fluent interface</returns>
		IStateMachineBuilder<TStateId, TParamId, TMessageId> SetInt(TParamId param, int value);
		
		/// <summary>
		/// Sets the value of a string parameter
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <param name="value">Value</param>
		/// <returns>Returns a fluent interface</returns>
		IStateMachineBuilder<TStateId, TParamId, TMessageId> SetString(TParamId param, string value);
		
		/// <summary>
		/// Sets a trigger parameter
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <returns>Returns a fluent interface</returns>
		IStateMachineBuilder<TStateId, TParamId, TMessageId> SetTrigger(TParamId param);
	}

	public interface IAddState<TStateId, TParamId, TMessageId> {
		
		/// <summary>
		/// Adds a new state to the state machine
		/// </summary>
		/// <param name="name">State id</param>
		/// <param name="state">State</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> AddState(TStateId name, State<TStateId, TParamId, TMessageId> state);
		
		/// <summary>
		/// Adds a new base state to the state machine
		/// </summary>
		/// <param name="name">State id</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> AddState(TStateId name);
	}

	public partial interface IAddTransition<TStateId, TParamId, TMessageId> {

		/// <summary>
		/// Adds a new transition that goes to the specified state
		/// </summary>
		/// <param name="state">State id of state to go to</param>
		/// <returns>Returns a fluent interface</returns>
		IAddCondition<TStateId, TParamId, TMessageId> GoTo(TStateId state);

		/// <summary>
		/// Adds a new transition that pushes the specified state onto the active states stack
		/// </summary>
		/// <param name="state">State id of state to push</param>
		/// <returns>Returns a fluent interface</returns>
		IAddCondition<TStateId, TParamId, TMessageId> Push(TStateId state);

		/// <summary>
		/// Adds a new transition that pops the peek state from the active states stack
		/// </summary>
		IAddCondition<TStateId, TParamId, TMessageId> Pop { get; }
	}


	public partial interface IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> : IAddTransition<TStateId, TParamId, TMessageId>, IAddState<TStateId, TParamId, TMessageId> {

		#region Handler setters

		/// <summary>
		/// Register a custom message handler
		/// </summary>
		/// <param name="id">Message id</param>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> On(TMessageId id, Action action);
	
		/// <summary>
		/// Register a custom message handler
		/// </summary>
		/// <param name="id">Message id</param>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> On(TMessageId id, Action<StateMachine<TStateId, TParamId, TMessageId>> action);
	
		/// <summary>
		/// Register a custom message handler
		/// </summary>
		/// <param name="id">Message id</param>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> On(TMessageId id, Action<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>> action);
	
		/// <summary>
		/// Register a custom message handler
		/// </summary>
		/// <param name="id">Message id</param>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> On(TMessageId id, StateMachine<TStateId, TParamId, TMessageId>.OnMessageHandler action);

		/// <summary>
		/// Register an on enter handler
		/// </summary>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnEnter(Action action);

		/// <summary>
		/// Register an on enter handler
		/// </summary>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnEnter(Action<StateMachine<TStateId, TParamId, TMessageId>> action);

		/// <summary>
		/// Register an on enter handler
		/// </summary>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnEnter(StateMachine<TStateId, TParamId, TMessageId>.OnTransitionHandler action);

		/// <summary>
		/// Register an on exit handler
		/// </summary>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnExit(Action action);

		/// <summary>
		/// Register an on exit handler
		/// </summary>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnExit(Action<StateMachine<TStateId, TParamId, TMessageId>> action);

		/// <summary>
		/// Register an on exit handler
		/// </summary>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnExit(StateMachine<TStateId, TParamId, TMessageId>.OnTransitionHandler action);

		/// <summary>
		/// Register an on pause handler
		/// </summary>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnPause(Action action);

		/// <summary>
		/// Register an on pause handler
		/// </summary>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnPause(Action<StateMachine<TStateId, TParamId, TMessageId>> action);

		/// <summary>
		/// Register an on pause handler
		/// </summary>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnPause(StateMachine<TStateId, TParamId, TMessageId>.OnTransitionHandler action);
	
		/// <summary>
		/// Register an on resume handler
		/// </summary>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnResume(Action action);

		/// <summary>
		/// Register an on resume handler
		/// </summary>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnResume(Action<StateMachine<TStateId, TParamId, TMessageId>> action);

		/// <summary>
		/// Register an on resume handler
		/// </summary>
		/// <param name="action">Handler</param>
		/// <returns>Returns a fluent interface</returns>
		IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnResume(StateMachine<TStateId, TParamId, TMessageId>.OnTransitionHandler action);

		#endregion

		/// <summary>
		/// Specifies that the next transition can occur from any of the specified states
		/// </summary>
		/// <param name="states">State ids to transition from</param>
		/// <returns>Returns a fluent interface</returns>
		IAddTransition<TStateId, TParamId, TMessageId> From(params TStateId[] states);

		/// <summary>
		/// Specifies that the next transition is global and can occur from any state
		/// </summary>
		IAddTransition<TStateId, TParamId, TMessageId> FromAny { get; }

		/// <summary>
		/// Returns the created state machine as the base state machine class
		/// </summary>
		StateMachine<TStateId, TParamId, TMessageId> Build { get; } // Finalizer
	}

	public interface IAddConditionAddHandlerAddTransitionAddStateBuildAddOr<TStateId, TParamId, TMessageId> : IAddCondition<TStateId, TParamId, TMessageId>, IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> {
		
		/// <summary>
		/// Specifies that the subsequent condition(s) can satisfy the transition irregardless of other conditions
		/// </summary>
		IAddCondition<TStateId, TParamId, TMessageId> Or { get; }
	}

	public interface IAddCondition<TStateId, TParamId, TMessageId> {

		/// <summary>
		/// Adds a bool condition to the most recently added transition
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <param name="check">Function to check if parameter meets the condition</param>
		/// <returns>Returns a fluent interface</returns>
		IAddConditionAddHandlerAddTransitionAddStateBuildAddOr<TStateId, TParamId, TMessageId> WhenBool(TParamId param, Func<bool, bool> check);

		/// <summary>
		/// Adds a float condition to the most recently added transition
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <param name="check">Function to check if parameter meets the condition</param>
		/// <returns>Returns a fluent interface</returns>
		IAddConditionAddHandlerAddTransitionAddStateBuildAddOr<TStateId, TParamId, TMessageId> WhenFloat(TParamId param, Func<float, bool> check);
	
		/// <summary>
		/// Adds an int condition to the most recently added transition
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <param name="check">Function to check if parameter meets the condition</param>
		/// <returns>Returns a fluent interface</returns>
		IAddConditionAddHandlerAddTransitionAddStateBuildAddOr<TStateId, TParamId, TMessageId> WhenInt(TParamId param, Func<int, bool> check);
		
		/// <summary>
		/// Adds a string condition to the most recently added transition
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <param name="check">Function to check if parameter meets the condition</param>
		/// <returns>Returns a fluent interface</returns>
		IAddConditionAddHandlerAddTransitionAddStateBuildAddOr<TStateId, TParamId, TMessageId> WhenString(TParamId param, Func<string, bool> check);
	
		/// <summary>
		/// Adds a trigger condition to the most recently added transition
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <returns>Returns a fluent interface</returns>
		IAddConditionAddHandlerAddTransitionAddStateBuildAddOr<TStateId, TParamId, TMessageId> WhenTrigger(TParamId param);

		/// <summary>
		/// Adds an observable condition to the most recently added transition
		/// </summary>
		/// <typeparam name="T">Observable value type</typeparam>
		/// <param name="param">Observable</param>
		/// <returns>Returns a fluent interface</returns>
		IAddConditionAddHandlerAddTransitionAddStateBuildAddOr<TStateId, TParamId, TMessageId> When<T>(Observable<T> param, Func<Observable<T>, bool> check);
	}
}
