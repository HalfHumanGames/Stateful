using System;
using Stateful;
using Stateful.Utilities;

namespace TicTacToeExample {

	// This is our root state machine class that includes the configuration and
	// a couple of methods used by main to handle drawing states and handling input.
	public class TicTacToe : StateMachine<StateId, ParamId> {

		protected override IAddHandlerAddTransitionAddStateBuild<StateId, ParamId, string> GetConfiguration() {
			// For simple state machines you can configure the entire state machine
			// within the constructor of the root state machine. For more complex
			// state machines, each substate machine can configure itself.
			return Builder. // Builder just creates a new StateMachineBuilder
				AddState(StateId.StartScreen, new StartScreen()).
					GoTo(StateId.GameScreen).
						WhenTrigger(ParamId.StartGame).
				AddState(StateId.GameScreen, Builder. // This is a substate machine
					AddState(StateId.GameScreen_Player1Turn, new PlayerTurnScreen(1)).
						GoTo(StateId.GameScreen_Player2Turn).
							WhenTrigger(ParamId.EndTurn).
					AddState(StateId.GameScreen_Player2Turn, new PlayerTurnScreen(2)).
						GoTo(StateId.GameScreen_Player1Turn).
							WhenTrigger(ParamId.EndTurn).
					AddState(StateId.GameScreen_GameOver, new GameOverScreen()).
					From(StateId.GameScreen_Player1Turn, StateId.GameScreen_Player2Turn).
						GoTo(StateId.GameScreen_GameOver). // You could also do when GameStatus !InProgress
							WhenInt(ParamId.GameStatus, x => x == (int) GameStatus.Player1Won).
							Or.WhenInt(ParamId.GameStatus, x => x == (int) GameStatus.Player2Won).
							Or.WhenInt(ParamId.GameStatus, x => x == (int) GameStatus.Draw).
					FromAny. // From and FromAny prevent us from having to rewrite code
						GoTo(StateId.StartScreen).
							WhenTrigger(ParamId.GoToStartScreen).
					Build<GameScreen>()); // Optionally specify the substate machine type
		}

		// Used in main to draw the active state
		public void Draw() {
			Draw(ActiveState as ITicTacToeScreen);
		}

		// We pull this out as another method so the GameScreen can access it
		public void Draw(ITicTacToeScreen state) {
			Print.Log(state.Title.ToUpper());
			Print.Log("-----------------------");
			state.Draw(this);
		}

		// Used in main to handle input for the active state
		public void HandleInput() {
			string input = Console.ReadLine();
			(ActiveState as ITicTacToeScreen).HandleInput(this, input);
		}
	}
}
