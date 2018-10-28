using StateMachineNet;

namespace ArcadeExample {

	public class StartScreen : ArcadeScreen {

		protected override string Title => "Zombie Blaster!";
		protected override string Description {
			get {
				// Notice that the description is dynamic and makes use of a parameter
				int coinsInserted = arcade.GetInt(ParamId.CoinsInserted);
				return $"Insert {coinsInserted}/{Arcade.CoinsRequired} coins to play";
			}
		}
		protected override string[] Options => new string[] {
			"Insert coin",
			"Unplug machine"
		};

		public override void HandleInput(Arcade stateMachine, string input) {
			if (input == "1") {

				// Insert a coin
				int coinsInserted = arcade.GetInt(ParamId.CoinsInserted);
				coinsInserted++;
				stateMachine.SetInt(ParamId.CoinsInserted, coinsInserted);
			} else if (input == "2") {

				// Stop the state machine
				stateMachine.Stop();
			}
		}

		// When the game starts, reset the number of coins inserted to 0
		protected override void OnExit(StateMachine<StateId, ParamId> stateMachine) =>
			stateMachine.SetInt(ParamId.CoinsInserted, 0);
	}
}
