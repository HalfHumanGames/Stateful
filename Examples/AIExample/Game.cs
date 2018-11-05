using System;
using System.Collections.Generic;
using Stateful.Utilities;

namespace AIExample {

	public class Game {

		public bool IsRunning = true;
		private (int x, int y) player;
		private (int x, int y) monster;
		private char[,] level;
		private const int size = 10;
		private AI ai;

		public Game() {
			level = new char[size, size];
			player = (0, size - 1);
			monster = (size - 1, 0);
			level[player.x, player.y] = 'P';
			level[monster.x, monster.y] = 'M';
			ai = new AI(
				// path definition
				new List<(int x, int y)>() {
					(size - 1, 4),
					(size - 5, 4),
					(size - 5, 0),
					(size - 1, 0)
				},
				monster,
				4, // aggro distance 
				8  // leash distance
			);
			ai.Start();
		}

		public void Draw() {

			if (!ai.LogFlow) {
				Console.Clear();
			}
			
			// title and controls
			string gui = "\n__________STAY ALIVE!_________\n\n        WASD to move.\n\n";

			// level and entities
			for (int y = size - 1; y >= 0; y--) {
				for (int x = 0; x < size; x++) {
					gui += $"[{level[x, y]}]";
				}
				gui += "\n";
			}

			// Display AI state
			gui += $"\n    Monster state: {ai.ActiveStateId}\n";

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
		
		// Absolute movement
		private void Move(ref (int x, int y) entity, (int x, int y) target, char character) {
			target.x = Math.Clamp(target.x, 0, level.GetLength(0) - 1);
			target.y = Math.Clamp(target.y, 0, level.GetLength(0) - 1);
			if (level[target.x, target.y] == '\0') {
				level[entity.x, entity.y] = '\0';
				entity.x = target.x;
				entity.y = target.y;
				level[entity.x, entity.y] = character;
			}
		}

		private void HandleMonsterMovement() => Move(ref monster, ai.HandleAI(player, monster), 'M');
	}
}
