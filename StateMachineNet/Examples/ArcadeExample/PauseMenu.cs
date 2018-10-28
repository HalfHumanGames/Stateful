using StateMachineNet;

namespace ArcadeExample {

	public class PauseMenu : ArcadeScreen {

		protected override string Title => "Game paused";
		protected override string Description => null;
		protected override string[] Options => new string[] {
			"Resume game",
			"Exit to start screen"
		};

		public override void HandleInput(Arcade stateMachine, string input) {
			if (input == "1") {

				// Unpause and resume gameplay
				stateMachine.SetBool(ParamId.IsPaused, false);
			} else if (input == "2") {

				// Exit to the start screen
				stateMachine.SetTrigger(ParamId.ExitToStartScreen);
			}
		}

		// Make sure to unpause on exit so the player does not re-enter the game
		// screen with the pause menu showing!
		protected override void OnExit(StateMachine<StateId, ParamId> stateMachine) =>
			stateMachine.SetBool(ParamId.IsPaused, false);
	}
}
