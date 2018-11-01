using System;
using System.Collections.Generic;
using StateMachineNet.Utilities;

namespace AIExample {

	public class Game {

		public bool IsRunning = true;
		private (int x, int y) playerPosition;
		private (int x, int y) monsterPosition;
		private char[,] level;
		private const int mapSize = 10;
		private AI monster;

		public Game() {
			level = new char[mapSize, mapSize];
			playerPosition = (0, mapSize - 1);
			monsterPosition = (mapSize - 1, 0);
			level[playerPosition.x, playerPosition.y] = 'P';
			level[monsterPosition.x, monsterPosition.y] = 'M';
			monster = new AI(
				new List<(int x, int y)>() { 
					(mapSize - 1, 4),
					(mapSize - 5, 4),
					(mapSize - 5, 0),
					(mapSize - 1, 0)
				},
				2, // aggro distance 
				8  // leash distance
			);
		}

		public void Draw() {
			Console.Clear();
			string gui = "STAY ALIVE!\nWASD to move.\n\n";
			for (int y = mapSize - 1; y >= 0; y--) {
				for (int x = 0; x < mapSize; x++) {
					gui += $"[{level[x, y]}]";
				}
				gui += "\n";
			}
			gui += "\nPress (q) to quit.";
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
			level[playerPosition.x, playerPosition.y] = '\0';
			switch (input) {
				case 'w': playerPosition.y++; break;
				case 'a': playerPosition.x--; break;
				case 's': playerPosition.y--; break;
				case 'd': playerPosition.x++; break;
			}
			playerPosition.x = Math.Clamp(playerPosition.x, 0, level.GetLength(0) - 1);
			playerPosition.y = Math.Clamp(playerPosition.y, 0, level.GetLength(0) - 1);
			level[playerPosition.x, playerPosition.y] = 'P';
		}

		private void HandleMonsterMovement() {
			monster.HandleAI(playerPosition, monsterPosition);
		}
	}
}
