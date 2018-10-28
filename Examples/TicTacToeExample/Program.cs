using System;
using StateMachineNet;
using StateMachineNet.Utilities;

namespace TicTacToeExample {

	// Game screens with underscores denoting child screens
	public enum StateId {
		StartScreen,
		GameScreen,
		GameScreen_Player1Turn,
		GameScreen_Player2Turn,
		GameScreen_GameOver
	}

	// Parameters used to control the state machine flow
	public enum ParamId {
		StartGame,
		EndTurn,
		GameOver,
		GameStatus,
		GoToStartScreen
	}

	// Game statuses a Tic-Tac-Toe game can have with all but
	// InProgress being end game conditions used to determine
	// when to go to the game over screen and how to draw it.
	public enum GameStatus {
		InProgress,
		Player1Won,
		Player2Won,
		Draw
	}

	public class Program {

		private static void Main(string[] args) {

			// The constructor configures the state machine
			TicTacToe ticTacToe = new TicTacToe();

			// Turn on to log the state machine flow when debugging
			ticTacToe.LogFlow.Value = true;

			// Start the state machine
			ticTacToe.Start();

			// While running draw the active state and handle input
			while (ticTacToe.IsRunning) {
				if (!ticTacToe.LogFlow.Value) {
					Console.Clear();
				}
				ticTacToe.Draw(); // Wrapper for ActiveState.Draw
				ticTacToe.HandleInput(); // Wrapper for ActiveState.HandleInput
			}
		}
	}
}
