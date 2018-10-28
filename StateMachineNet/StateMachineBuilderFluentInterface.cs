using System;
using StateMachineNet.Utilities;

namespace StateMachineNet {

	public interface IStateMachineBuilderFluentInterface<TStateId, TParamId> : IStateMachineBuilder<TStateId, TParamId>, IAddConditionAddTransitionAddStateBuildAddOr<TStateId, TParamId> { }

	public interface IStateMachineBuilder<TStateId, TParamId> : IAddState<TStateId, TParamId> {
		
		/// <summary>
		/// Sets the value of a bool parameter
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <param name="value">Value</param>
		/// <returns>Returns a fluent interface</returns>
		IStateMachineBuilder<TStateId, TParamId> SetBool(TParamId param, bool value);
		
		/// <summary>
		/// Sets the value of a float parameter
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <param name="value">Value</param>
		/// <returns>Returns a fluent interface</returns>
		IStateMachineBuilder<TStateId, TParamId> SetFloat(TParamId param, float value);
		
		/// <summary>
		/// Sets the value of an int parameter
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <param name="value">Value</param>
		/// <returns>Returns a fluent interface</returns>
		IStateMachineBuilder<TStateId, TParamId> SetInt(TParamId param, int value);
		
		/// <summary>
		/// Sets the value of a string parameter
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <param name="value">Value</param>
		/// <returns>Returns a fluent interface</returns>
		IStateMachineBuilder<TStateId, TParamId> SetString(TParamId param, string value);
		
		/// <summary>
		/// Sets a trigger parameter
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <returns>Returns a fluent interface</returns>
		IStateMachineBuilder<TStateId, TParamId> SetTrigger(TParamId param);
	}

	public interface IAddState<TStateId, TParamId> {
		
		/// <summary>
		/// Adds a new state to the state machine
		/// </summary>
		/// <param name="name">State id</param>
		/// <param name="state">State</param>
		/// <returns>Returns a fluent interface</returns>
		IAddTransitionAddStateBuild<TStateId, TParamId> AddState(TStateId name, State<TStateId, TParamId> state);
		
		/// <summary>
		/// Adds a new base state to the state machine
		/// </summary>
		/// <param name="name">State id</param>
		/// <returns>Returns a fluent interface</returns>
		IAddTransitionAddStateBuild<TStateId, TParamId> AddState(TStateId name);
	}

	public interface IAddTransition<TStateId, TParamId> {

		/// <summary>
		/// Adds a new transition that goes to the specified state
		/// </summary>
		/// <param name="state">State id of state to go to</param>
		/// <returns>Returns a fluent interface</returns>
		IAddCondition<TStateId, TParamId> GoTo(TStateId state);

		/// <summary>
		/// Adds a new transition that pushes the specified state onto the active states stack
		/// </summary>
		/// <param name="state">State id of state to push</param>
		/// <returns>Returns a fluent interface</returns>
		IAddCondition<TStateId, TParamId> Push(TStateId state);

		/// <summary>
		/// Adds a new transition that pops the peek state from the active states stack
		/// </summary>
		IAddCondition<TStateId, TParamId> Pop { get; }
	}

	public interface IAddTransitionAddStateBuild<TStateId, TParamId> : IAddTransition<TStateId, TParamId>, IAddState<TStateId, TParamId> {

		IAddTransitionAddStateBuild On<T>(T id, Action<StateMachine<TStateId, TParamId>> action);

		/// <summary>
		/// Specifies that the next transition can occur from any of the specified states
		/// </summary>
		/// <param name="states">State ids to transition from</param>
		/// <returns>Returns a fluent interface</returns>
		IAddTransition<TStateId, TParamId> From(params TStateId[] states);

		/// <summary>
		/// Specifies that the next transition is global and can occur from any state
		/// </summary>
		IAddTransition<TStateId, TParamId> FromAny { get; }

		/// <summary>
		/// Returns the created state machine as the base state machine class
		/// </summary>
		StateMachine<TStateId, TParamId> Build { get; }
	}

	public interface IAddConditionAddTransitionAddStateBuildAddOr<TStateId, TParamId> : IAddCondition<TStateId, TParamId>, IAddTransitionAddStateBuild<TStateId, TParamId> {
		
		/// <summary>
		/// Specifies that the subsequent condition(s) can satisfy the transition irregardless of other conditions
		/// </summary>
		IAddCondition<TStateId, TParamId> Or { get; }
	}

	public interface IAddCondition<TStateId, TParamId> {

		/// <summary>
		/// Adds a bool condition to the most recently added transition
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <param name="check">Function to check if parameter meets the condition</param>
		/// <returns>Returns a fluent interface</returns>
		IAddConditionAddTransitionAddStateBuildAddOr<TStateId, TParamId> WhenBool(TParamId param, Func<bool, bool> check);

		/// <summary>
		/// Adds a float condition to the most recently added transition
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <param name="check">Function to check if parameter meets the condition</param>
		/// <returns>Returns a fluent interface</returns>
		IAddConditionAddTransitionAddStateBuildAddOr<TStateId, TParamId> WhenFloat(TParamId param, Func<float, bool> check);
	
		/// <summary>
		/// Adds an int condition to the most recently added transition
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <param name="check">Function to check if parameter meets the condition</param>
		/// <returns>Returns a fluent interface</returns>
		IAddConditionAddTransitionAddStateBuildAddOr<TStateId, TParamId> WhenInt(TParamId param, Func<int, bool> check);
		
		/// <summary>
		/// Adds a string condition to the most recently added transition
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <param name="check">Function to check if parameter meets the condition</param>
		/// <returns>Returns a fluent interface</returns>
		IAddConditionAddTransitionAddStateBuildAddOr<TStateId, TParamId> WhenString(TParamId param, Func<string, bool> check);
	
		/// <summary>
		/// Adds a trigger condition to the most recently added transition
		/// </summary>
		/// <param name="param">Parameter id</param>
		/// <returns>Returns a fluent interface</returns>
		IAddConditionAddTransitionAddStateBuildAddOr<TStateId, TParamId> WhenTrigger(TParamId param);

		/// <summary>
		/// Adds an observable condition to the most recently added transition
		/// </summary>
		/// <typeparam name="T">Observable value type</typeparam>
		/// <param name="param">Observable</param>
		/// <returns>Returns a fluent interface</returns>
		IAddConditionAddTransitionAddStateBuildAddOr<TStateId, TParamId> When<T>(Observable<T> param, Func<Observable<T>, bool> check);
	}
}
