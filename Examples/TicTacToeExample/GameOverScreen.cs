using Stateful;
using Stateful.Utilities;

namespace TicTacToeExample {

	public class GameOverScreen : State<StateId, ParamId>, ITicTacToeScreen {

		public string Title => "Game Over";

		public void Draw(StateMachine<StateId, ParamId> stateMachine) {

			// Draw the outcome of the game and ask the player if they want
			// to go back to the Start Screen (or continue viewing the board)
			GameStatus gameStatus = (GameStatus) stateMachine.GetInt(ParamId.GameStatus);
			switch (gameStatus) {
				case GameStatus.Draw:
					Print.Log("Draw game!");
					break;
				case GameStatus.Player1Won:
					Print.Log("Player 1 wins!");
					break;
				case GameStatus.Player2Won:
					Print.Log("Player 2 wins!");
					break;
			}
			Print.Log("Go back to Start Screen? (y/n)");
		}

		public void HandleInput(StateMachine<StateId, ParamId> stateMachine, string input) {

			// When ready, take the player back to the state screen
			if (input == "y") {
				stateMachine.SetTrigger(ParamId.GoToStartScreen);
			}
		}
	}
}
