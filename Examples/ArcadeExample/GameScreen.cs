using Stateful;

namespace ArcadeExample {

	// This screen is where the gameplay takes place
	public class GameScreen : ArcadeScreen {

		protected override string Title => "Game on!";
		protected override string Description {
			get {
				// Here is an example of using state machine parameters in a screen description
				int hearts = arcade.GetInt(ParamId.Hearts);
				int zombiesKilled = arcade.GetInt(ParamId.ZombiesKilled);
				return $"Hearts: {hearts}\nKills: {zombiesKilled}/{Arcade.NumZombies}";
			}
		}

		protected override string[] Options => new string[] {
			"Kill zombie",
			"Die cuz u suck",
			"Pause game"
		};

		public override void HandleInput(Arcade stateMachine, string input) {

			if (input == "1") {

				// Kill a zombie
				int zombiesKilled = stateMachine.GetInt(ParamId.ZombiesKilled);
				zombiesKilled++;
				stateMachine.SetInt(ParamId.ZombiesKilled, zombiesKilled);
			} else if (input == "2") {

				// Lose a heart/life
				int hearts = stateMachine.GetInt(ParamId.Hearts);
				hearts--;
				stateMachine.SetInt(ParamId.Hearts, hearts);

			} else if (input == "3") {

				// Open the pause menu. Remember that we configured this as a Push
				// transition meaning that the game screen retains it state so when
				// we unpause (aka Pop the pause menu
				stateMachine.SetBool(ParamId.IsPaused, true);
			}
		}

		protected override void OnEnter(StateMachine<StateId, ParamId, string> stateMachine) {

			// On Enter reset the state of the game
			base.OnEnter(stateMachine);
			stateMachine.SetInt(ParamId.Hearts, Arcade.StartingHearts);
			stateMachine.SetInt(ParamId.ZombiesKilled, 0);
		}
	}
}
