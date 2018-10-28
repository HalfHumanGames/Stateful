using StateMachineNet;
using StateMachineNet.Utilities;
using System.Threading;

namespace HelloWorldExample {

	public class Program {

		private static void Main(string[] args) {

			// Build a state machine with two states: "Hello, world!" and "Goodbye, world!"
			// Toggle between these two states when the trigger "Toggle state" gets set.
			// Don't worry, you can use whatever data type you'd like for the state and param ids.
			StateMachine stateMachine = StateMachineBuilder.Create().
				AddState("Hello, world!").GoTo("Goodbye, world!").WhenTrigger("Toggle state").
				AddState("Goodbye, world!").GoTo("Hello, world!").WhenTrigger("Toggle state").
				Build.As<StateMachine>();

			// Enable logging so you can see what goes on inside the state machine
			stateMachine.LogFlow.Value = true;

			// Start the state machine, aka enter the first state: "Hello, world!"
			stateMachine.Start();

			// Toggle states by setting the "Toggle state" trigger every second 5 times
			int count = 5;
			while (count-- >= 0) {
				Print.Log(stateMachine.ActiveStateId);
				Thread.Sleep(1000);
				stateMachine.SetTrigger("Toggle state");
			}
		}
	}
}
