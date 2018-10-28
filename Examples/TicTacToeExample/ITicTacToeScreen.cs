using StateMachineNet;

namespace TicTacToeExample {

	// Every Tic-Tac-Toe screen has to have these properties/methods
	public interface ITicTacToeScreen {

		// Automatically drawn by TicTacToe
		string Title { get; }

		// Renders text for screen
		void Draw(StateMachine<StateId, ParamId> stateMachine);

		// Handles input for screen
		void HandleInput(StateMachine<StateId, ParamId> stateMachine, string input);
	}
}
