using Stateful;
using Stateful.Utilities;

namespace TicTacToeExample {

	// This screen just prompts the player for input and updates the board accordingly
	public class PlayerTurnScreen : State<StateId, ParamId>, ITicTacToeScreen {

		public string Title => $"Player {PlayerId}'s Turn";
		public int PlayerId { get; private set; }
		public char Mark => PlayerId == 1 ? 'X' : 'O';

		public PlayerTurnScreen(int playerId) => PlayerId = playerId;

		// Prompt the player for two integers, i.e. "1 2" or "2 0"
		public void Draw(StateMachine<StateId, ParamId> stateMachine) =>
			Print.Log("Which cell do you want to mark next? (row col)");

		public void HandleInput(StateMachine<StateId, ParamId> stateMachine, string input) {

			string[] rowCol = input.Trim().Split(' ');

			// Make sure player provided two inputs
			if (rowCol.Length < 2) {
				return;
			}

			// Make sure that each input is an integer and within the board's bounds
			bool success;
			success = int.TryParse(rowCol[0], out int row);
			if (!success || row < 0 || row > 2) {
				return;
			}
			success = int.TryParse(rowCol[1], out int col);
			if (!success || col < 0 || col > 2) {
				return;
			}

			// Make sure that the specified board cell does not already contain a mark
			GameScreen gameScreen = stateMachine as GameScreen;
			if (gameScreen.Board[row, col] != '\0') {
				return;
			}

			// Mark the board cell and end this player's turn
			gameScreen.Board[row, col] = Mark;
			stateMachine.SetTrigger(ParamId.EndTurn);
		}
	}
}
