namespace ArcadeExample {

	public class Program {

		private static void Main(string[] args) {

			// The constructor configures the state machine
			Arcade arcade = new Arcade {
				// Turn on to log the state machine flow when debugging
				LogFlow = true
			};

			// Start the state machine
			arcade.Start();

			// While running draw the active state and handle input
			while (arcade.IsRunning) {
				arcade.Draw();		  // Wrapper for ActiveState.Draw
				arcade.HandleInput(); // Wrapper for ActiveState.HandleInput
			}
		}
	}
}
