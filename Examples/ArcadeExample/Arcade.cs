using StateMachineNet;
using System;

namespace ArcadeExample {

	public class Arcade : StateMachine<StateId, ParamId> {

		public const int CoinsRequired = 4;
		public const int NumZombies = 3;
		public const int StartingHearts = 3;

		public Arcade() => Configure(
			Builder.
				SetInt(ParamId.CoinsInserted, 2).
				AddState(StateId.StartScreen, new StartScreen()).
					GoTo(StateId.GameScreen).
						WhenInt(ParamId.CoinsInserted, x => x >= CoinsRequired).
				AddState(StateId.GameScreen, new GameScreen()).
					GoTo(StateId.WinScreen).
						WhenInt(ParamId.ZombiesKilled, x => x >= NumZombies).
					GoTo(StateId.LoseScreen).
						WhenInt(ParamId.Hearts, x => x <= 0).
					Push(StateId.PauseMenu).
						WhenBool(ParamId.IsPaused, x => x).
				AddState(StateId.WinScreen, new WinScreen()).
					GoTo(StateId.StartScreen).
						WhenTrigger(ParamId.Continue).
				AddState(StateId.LoseScreen, new LoseScreen()).
					GoTo(StateId.StartScreen).
						WhenTrigger(ParamId.Continue).
				AddState(StateId.PauseMenu, new PauseMenu()).
					Pop.
						WhenBool(ParamId.IsPaused, x => !x).
					GoTo(StateId.StartScreen).
						WhenTrigger(ParamId.ExitToStartScreen)
		);

		public void Draw() => (ActiveState as ArcadeScreen).Draw(this);

		public void HandleInput() {
			string input = Console.ReadLine();
			(ActiveState as ArcadeScreen).HandleInput(this, input);
		}
	}
}
