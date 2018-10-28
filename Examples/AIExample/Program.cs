using System;

namespace AIExample {

	public class Program {

		private static void Main(string[] args) {
			
			Game game = new Game();
			while (game.IsRunning) {
				game.Draw();
				game.Update(Console.ReadKey().KeyChar);
			}
		}
	}
}
