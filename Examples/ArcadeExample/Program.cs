namespace ArcadeExample {

	// Game screens
	public enum StateId {
		GameScreen,
		LoseScreen,
		PauseMenu,
		StartScreen,
		WinScreen
	}

	// Parameters used to control the state machine flow
	public enum ParamId {
		CoinsInserted,
		Continue,
		Hearts,
		IsPaused,
		ZombiesKilled,
		ExitToStartScreen
	}

	public class Program {

		private static void Main(string[] args) {

			// The constructor configures the state machine
			Arcade arcade = new Arcade();

			// Turn on to log the state machine flow when debugging
			arcade.LogFlow.Value = true;

			// Start the state machine
			arcade.Start();

			// While running draw the active state and handle input
			while (arcade.IsRunning) {
				arcade.Draw(); // Wrapper for ActiveState.Draw
				arcade.HandleInput(); // Wrapper for ActiveState.HandleInput
			}
		}
	}
}
