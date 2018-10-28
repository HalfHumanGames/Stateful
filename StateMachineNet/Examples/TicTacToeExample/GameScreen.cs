using StateMachineNet;
using StateMachineNet.Utilities;

namespace TicTacToeExample {

	// This screen resets the game status and game board on enter. Each update it draws the
	// child screen and then the game board. After the child screen processes player input,
	// the game screen examines the board and determines the current game state.
	public class GameScreen : StateMachine<StateId, ParamId>, ITicTacToeScreen {

		public string Title => "Game on!";
		public char[,] Board;

		protected override void OnEnter(StateMachine<StateId, ParamId> stateMachine) {

			// Reset the game state prior to calling base.Enter to prevent falling through
			// PlayerTurn and going straight to GameOverScreen instead. Alternatively, 
			// you can reset the board and GameStatus to InProgress when exiting GameScreen
			stateMachine.SetInt(ParamId.GameStatus, (int) GameStatus.InProgress);
			Board = new char[3, 3];
			base.OnEnter(stateMachine);
		}

		public void Draw(StateMachine<StateId, ParamId> stateMachine) {

			// The parent Tic-Tac-Toe state machine draws the Game Screen's title and then
			// calls this method so now we just have to draw the Game Screen's active state
			(stateMachine as TicTacToe).Draw(ActiveState as ITicTacToeScreen);

			// Draw game board
			string hud = "";
			for (int row = 0; row < 3; row++) {
				for (int col = 0; col < 3; col++) {
					char mark = Board[row, col] == '\0' ? ' ' : Board[row, col];
					hud += $"[{mark}]";
				}
				hud += "\n";
			}
			Print.Log(hud);
		}

		public void HandleInput(StateMachine<StateId, ParamId> stateMachine, string input) {

			// Handle input for the current screen
			(ActiveState as ITicTacToeScreen).HandleInput(this, input);

			// If the game is in progress, check for game over conditions
			// We do not have to reference the parent state machine since
			// state machines and substate machiens share paremeters
			if (GetInt(ParamId.GameStatus) == (int) GameStatus.InProgress) {
				SetInt(ParamId.GameStatus, (int) GetGameStatus(Board));
			}
		}

		// Check game over conditions
		private GameStatus GetGameStatus(char[,] board) {

			bool win;

			// Check each row
			for (int i = 0; i < 3; i++) {
				win = IsWinningLine(board[i, 0], board[i, 1], board[i, 2]);
				if (win) {
					int winner = board[i, 0] == 'X' ? 1 : 0;
					return winner == 1 ? GameStatus.Player1Won : GameStatus.Player2Won;
				}
			}

			// Check each column
			for (int i = 0; i < 3; i++) {
				win = IsWinningLine(board[0, i], board[1, i], board[2, i]);
				if (win) {
					int winner = board[0, i] == 'X' ? 1 : 0;
					return winner == 1 ? GameStatus.Player1Won : GameStatus.Player2Won;
				}
			}

			// Check diagonals
			win = IsWinningLine(board[0, 0], board[1, 1], board[2, 2]);
			if (win) {
				int winner = board[0, 0] == 'X' ? 1 : 0;
				return winner == 1 ? GameStatus.Player1Won : GameStatus.Player2Won;
			}
			win = IsWinningLine(board[0, 2], board[1, 1], board[2, 0]);
			if (win) {
				int winner = board[0, 2] == 'X' ? 1 : 0;
				return winner == 1 ? GameStatus.Player1Won : GameStatus.Player2Won;
			}

			// Check for empty cell to see if any
			// player can still make another move
			for (int i = 0; i < 3; i++) {
				for (int j = 0; j < 3; j++) {
					if (board[i, j] == '\0') {
						return GameStatus.InProgress;
					}
				}
			}

			// The game is a draw game if there is no winner
			// and neither player can make another move
			return GameStatus.Draw;
		}

		// Helper method to check for three marks in a row
		private bool IsWinningLine(char i, char j, char k) => i != '\0' && i == j && j == k;
	}
}
