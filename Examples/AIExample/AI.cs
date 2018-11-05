using System;
using System.Collections.Generic;
using Stateful;
using Stateful.Utilities;

namespace AIExample {

	public enum StateId {
		Patrol,
		Aggro,
		Leash
	}

	public enum ParamId { DestinationReached }

	public enum MessageId { HandleAI }

	public class AI : StateMachine<StateId, ParamId, MessageId> {

		private struct Context {

			public (int x, int y) PlayerPosition;
			public (int x, int y) MonsterPosition;

			public Context((int x, int y) playerPosition, (int x, int y) monsterPosition) {
				PlayerPosition = playerPosition;
				MonsterPosition = monsterPosition;
			}
		}

		private int pathIndex;
		private (int x, int y) spawn;
		private List<(int x, int y)> path;
		private Observable<int> distanceFromSpawn = new Observable<int>(0);
		private Observable<int> distanceFromPlayer = new Observable<int>(int.MaxValue);

		// What happens when register observable multiple times???
		public AI(List<(int x, int y)> path, (int x, int y) spawn, int aggro, int leash) {
			this.spawn = spawn;
			this.path = path;
			Configure(
				Builder.
					AddState(StateId.Patrol).
						On(MessageId.HandleAI, Patrol_GetNextPosition).
					AddState(StateId.Aggro).
						On(MessageId.HandleAI, Aggro_GetNextPosition).
						GoTo(StateId.Leash).
							When(distanceFromSpawn, x => x >= leash).
					AddState(StateId.Leash).
						On(MessageId.HandleAI, Leash_GetNextPosition).
						GoTo(StateId.Patrol).
							WhenTrigger(ParamId.DestinationReached).
					From(StateId.Patrol, StateId.Leash).
						GoTo(StateId.Aggro).
							When(distanceFromSpawn, x => x < leash - 1).
							When(distanceFromPlayer, x => x <= aggro)
			);
		}

		public (int x, int y) HandleAI((int x, int y) playerPosition, (int x, int y) monsterPosition) {

			int deltaX = playerPosition.x - monsterPosition.x;
			int deltaY = playerPosition.y - monsterPosition.y;

			distanceFromPlayer.Value = Math.Abs(deltaX) + Math.Abs(deltaY);

			deltaX = spawn.x - monsterPosition.x;
			deltaY = spawn.y - monsterPosition.y;

			distanceFromSpawn.Value = Math.Abs(deltaX) + Math.Abs(deltaY);

			// Create new context to pass to ai handlers
			Context context = new Context(playerPosition, monsterPosition);

			// Active state determines next position
			return SendMessage<(int x, int y)>(MessageId.HandleAI, context);
		}

		private (int x, int y) Patrol_GetNextPosition(
			StateMachine<StateId, ParamId, MessageId> stateMachine,
			State<StateId, ParamId, MessageId> state,
			object data
		) {
			Context context = (Context) data;
			if (context.MonsterPosition.x == path[pathIndex].x &&
				context.MonsterPosition.y == path[pathIndex].y
			) {
				pathIndex = (pathIndex + 1) % path.Count;
			}
			return MoveTowards(context.MonsterPosition, path[pathIndex]);
		}

		private (int x, int y) Aggro_GetNextPosition(
			StateMachine<StateId, ParamId, MessageId> stateMachine,
			State<StateId, ParamId, MessageId> state,
			object data
		) {
			Context context = (Context) data;
			return MoveTowards(context.MonsterPosition, context.PlayerPosition);
		}

		private (int x, int y) Leash_GetNextPosition(
			StateMachine<StateId, ParamId, MessageId> stateMachine,
			State<StateId, ParamId, MessageId> state,
			object data
		) {
			Context context = (Context) data;
			if (context.MonsterPosition.x == path[pathIndex].x &&
				context.MonsterPosition.y == path[pathIndex].y
			) {
				SetTrigger(ParamId.DestinationReached);
			}
			return MoveTowards(context.MonsterPosition,  path[pathIndex]);
		}

		private (int x, int y) MoveTowards((int x, int y) monsterPosition, (int x, int y) targetPosition) {
			int deltaX = targetPosition.x - monsterPosition.x;
			int deltaY = targetPosition.y - monsterPosition.y;
			if (Math.Abs(deltaX) > Math.Abs(deltaY)) {
				deltaY = 0;
			} else {
				deltaX = 0;
			}
			deltaX = Math.Clamp(deltaX, -1, 1);
			deltaY = Math.Clamp(deltaY, -1, 1);
			monsterPosition.x += deltaX;
			monsterPosition.y += deltaY;
			return monsterPosition;
		}
	}
}

