using System.Collections.Generic;
using StateMachineNet;
using StateMachineNet.Utilities;

namespace AIExample {

	public enum StateId {
		Patrol,
		Aggro,
		Attack,
		Leash
	}

	public enum ParamId {

	}

	public enum MessageId {
		HandleAI
	}

	public class AI : StateMachine<StateId, ParamId, MessageId> {

		//private IEnumerator<(int, int)> pathEnumerator;
		private Observable<int> distanceFromPlayer = new Observable<int>();
		private Observable<bool> destinationReached = new Observable<bool>();

		// What happens when register observable multiple times???
		public AI(List<(int x, int y)> path, int aggro, int leash) => Configure(
			Builder.
				AddState(StateId.Patrol).
					On(MessageId.HandleAI, HandlePatrol).
				AddState(StateId.Aggro).
					On(MessageId.HandleAI, HandleAggro).
					GoTo(StateId.Attack).
						When(distanceFromPlayer, x => x.Value == 1).
					GoTo(StateId.Leash).
						When(distanceFromPlayer, x => x.Value >= leash).
				AddState(StateId.Attack).
					On(MessageId.HandleAI, HandleAttack).
					GoTo(StateId.Aggro).
						When(distanceFromPlayer, x => x.Value > 1).
				AddState(StateId.Leash).
					On(MessageId.HandleAI, HandleLeash).
					GoTo(StateId.Patrol).
						When(destinationReached, x => x.Value).
				From(StateId.Patrol, StateId.Leash).
					GoTo(StateId.Aggro).
						When(distanceFromPlayer, x => x.Value <= aggro)
		);

		public void HandleAI((int x, int y) playerPosition, (int x, int y) monsterPosition) =>
			SendMessage(MessageId.HandleAI);

		private void HandlePatrol() {

		}

		private void HandleAggro() {

		}

		private void HandleAttack() {

		}

		private void HandleLeash() {

		}
	}
}
