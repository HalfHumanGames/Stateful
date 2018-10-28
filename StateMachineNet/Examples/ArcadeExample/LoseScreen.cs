namespace ArcadeExample {

	public class LoseScreen : ArcadeScreen {

		protected override string Title => "You lose!";
		protected override string Description => null;
		protected override string[] Options => new string[] {
			"Go to start screen",
		};

		public override void HandleInput(Arcade stateMachine, string input) {
			if (input == "1") {

				// Go to start screen
				stateMachine.SetTrigger(ParamId.Continue);
			}
		}
	}
}
