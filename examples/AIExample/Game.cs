using System;
using Stateful.Utilities;

namespace AIExample {

	public class Game {

		private const int aggroDistance = 4;
		private const int leashDistance = 10;
		private const int levelSize = 10;
		private static readonly (int, int)[] pathDefinition = 
			new (int x, int y)[] {
				(levelSize - 1, 4),
				(levelSize - 5, 4),
				(levelSize - 5, 0),
				(levelSize - 1, 0)
			};

		public bool IsRunning = true;
		private (int x, int y) player;
		private (int x, int y) monster;
		private char[,] level;
		private AI ai;

		public Game() {
			level = new char[levelSize, levelSize];
			player = (0, levelSize - 1);
			monster = (levelSize - 1, 0);
			level[player.x, player.y] = 'P';
			level[monster.x, monster.y] = 'M';
			ai = new AI(
				pathDefinition,
				monster,
				aggroDistance,
				leashDistance
			);
			//ai.LogFlow = true;
			ai.Start();
		}

		public void Draw() {

			if (!ai.LogFlow) {
				Console.Clear();
			}
			
			// title and controls
			string gui = "\n__________STAY ALIVE!_________\n\n         WASD to move.\n\n";

			// level and entities
			for (int y = levelSize - 1; y >= 0; y--) {
				for (int x = 0; x < levelSize; x++) {
					gui += $"[{level[x, y]}]";
				}
				gui += "\n";
			}

			// Display AI state
			gui += $"\n     Monster state: {ai.ActiveStateId}\n";

			gui += $"\n     Aggro distance: {aggroDistance}";
			gui += $"\n     Leash distance: {leashDistance}\n";

			// q to quit
			gui += "\n     Press (q) to quit.";

			Print.Log(gui);
		}

		public void Update(char input) {
			if (input == 'q') {
				IsRunning = false;
				return;
			}
			HandlePlayerMovement(input);
			HandleMonsterMovement();
		}

		private void HandlePlayerMovement(char input) {
			int x = player.x;
			int y = player.y;
			switch (input) {
				case 'w': y++; break;
				case 'a': x--; break;
				case 's': y--; break;
				case 'd': x++; break;
			}
			Move(ref player, (x, y), 'P');
		}

		private void HandleMonsterMovement() => Move(ref monster, ai.HandleAI(player, monster), 'M');
		
		// Absolute movement
		private void Move(ref (int x, int y) entity, (int x, int y) target, char character) {
			// Math.Clamp not available in .NET Standard 2.0
			target.x = MathUtility.Clamp(target.x, 0, level.GetLength(0) - 1);
			target.y = MathUtility.Clamp(target.y, 0, level.GetLength(0) - 1);
			if (level[target.x, target.y] == '\0') {
				level[entity.x, entity.y] = '\0';
				entity.x = target.x;
				entity.y = target.y;
				level[entity.x, entity.y] = character;
			}
		}
	}
}
