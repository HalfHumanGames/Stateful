using Stateful;
using Stateful.Utilities;
using System;

namespace ArcadeExample {

	// Base class for all arcade states. If you have a substate machine,
	//  it might be better to use an interface or composition instead.
	public abstract class ArcadeScreen : State<StateId, ParamId> {

		// Every screen has a title, description, and options. The options show what the
		// player can do on a given screen. The Draw method automatically prints the
		// title, description, and options to the console for each screen.
		protected abstract string Title { get; }
		protected abstract string Description { get; }
		protected abstract string[] Options { get; }
		protected Arcade arcade;

		public void Draw(Arcade arcade) {

			// Do not clear the console if logging is enabled so we
			// can see the internal flow of the state machine
			if (!arcade.LogFlow) {
				Console.Clear();
			}

			// Print the title, description, and options
			Print.Log(Title.ToUpper());
			Print.Log("-----------------------");
			if (!string.IsNullOrEmpty(Description)) {
				Print.Log(Description);
				Print.Log("-----------------------");
			}
			Print.Log("What do you want to do?");
			for (int i = 0; i < Options.Length; i++) {
				Print.Log($"{i + 1}) {Options[i]}");
			}
		}

		// Every screen must have a means of handling the player input
		public abstract void HandleInput(Arcade arcadeMachine, string input);

		// On Enter, cache a reference to the state machine casted as an Arcade.
		// We do this so that we have access to the state machine when getting the
		// screen's title, description, or options in case we want to use parameters
		// in them. For example: "{LivesRemaining}/3 Lives"
		protected override void OnEnter(StateMachine<StateId, ParamId, string> stateMachine) =>
			arcade = stateMachine as Arcade;
	}
}
