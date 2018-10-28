using StateMachineNet;
using System;

namespace ArcadeExample {

	// This is our root state machine class that includes the configuration and
	// a couple of methods used by main to handle drawing states and handling input.
	public class Arcade : StateMachine<StateId, ParamId> {

		public const int CoinsRequired = 4;
		public const int NumZombies = 3;
		public const int StartingHearts = 3;

		public Arcade() => Configure(

			// For simple state machines you can configure the entire state machine
			// within the constructor of the root state machine. For more complex
			// state machines, each substate machine can configure itself.
			Builder.
				SetInt(ParamId.CoinsInserted, 2).
				AddState(StateId.StartScreen, new StartScreen()).
					GoTo(StateId.GameScreen).
						WhenInt(ParamId.CoinsInserted, x => x >= CoinsRequired).
				AddState(StateId.GameScreen, new GameScreen()).
					GoTo(StateId.WinScreen).
						WhenInt(ParamId.ZombiesKilled, x => x >= NumZombies).
					GoTo(StateId.LoseScreen).
						WhenInt(ParamId.Hearts, x => x <= 0).
					Push(StateId.PauseMenu).
						WhenBool(ParamId.IsPaused, x => x).
				AddState(StateId.WinScreen, new WinScreen()).
					GoTo(StateId.StartScreen).
						WhenTrigger(ParamId.Continue).
				AddState(StateId.LoseScreen, new LoseScreen()).
					GoTo(StateId.StartScreen).
						WhenTrigger(ParamId.Continue).
				AddState(StateId.PauseMenu, new PauseMenu()).
					Pop.
						WhenBool(ParamId.IsPaused, x => !x).
					GoTo(StateId.StartScreen).
						WhenTrigger(ParamId.ExitToStartScreen)
			// Building and casting (As) are optional for the root state machine
		);

		// Used in main to draw the active state
		public void Draw() => (ActiveState as ArcadeScreen).Draw(this);

		// Used in main to handle input for the active state
		public void HandleInput() {
			string input = Console.ReadLine();
			(ActiveState as ArcadeScreen).HandleInput(this, input);
		}
	}
}
