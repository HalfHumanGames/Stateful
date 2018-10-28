using StateMachineNet;
using StateMachineNet.Utilities;

namespace TicTacToeExample {

	public class StartScreen : State<StateId, ParamId>, ITicTacToeScreen {

		public string Title => "Tic-Tac-Toe";

		// Ask the players if they are ready to play
		public void Draw(StateMachine<StateId, ParamId> stateMachine) =>
			Print.Log("Are you ready to play? (y/n)");

		public void HandleInput(StateMachine<StateId, ParamId> stateMachine, string input) {

			// When they're ready, start the game
			if (input == "y") {
				stateMachine.SetTrigger(ParamId.StartGame);
			}
		}
	}
}
