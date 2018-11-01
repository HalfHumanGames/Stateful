using System;
using StateMachineNet;
using StateMachineNet.Utilities;

namespace SerializeExample {

	public class Program {

		private static void Main(string[] args) {

			StateMachine stateMachine = StateMachineBuilder.Create().
				AddState("First").
					GoTo("Second").WhenTrigger("Toggle").
				AddState("Second").
					GoTo("First").WhenTrigger("Toggle").
				Build.As<StateMachine>();

			stateMachine.LogFlow.Value = true;
			stateMachine.Start();

			// Serialize
			// Custom subclasses must have the Serializable attribute
			byte[] data = stateMachine.Serialize();

			// First state
			Print.Log($"1: {stateMachine.ActiveStateId}");

			// Go to second state
			stateMachine.SetTrigger("Toggle");

			// Second state
			Print.Log($"2: {stateMachine.ActiveStateId}");

			// Deserialize
			stateMachine.Deserialize(data);

			// First state
			Print.Log($"3: {stateMachine.ActiveStateId}");

			Console.ReadLine();
		}
	}
}
