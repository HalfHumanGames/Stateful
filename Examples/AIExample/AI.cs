using System.Collections.Generic;
using StateMachineNet;

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

	public class AI : StateMachine<string> {

		public AI(List<(int x, int y)> path, int aggro, int leash) => Configure(
			Builder.
				AddState("Patrol").
					//OnEnter((s) => { }).// have version w/ and w/out state machine param
					On(MessageId.HandleAI, (s) => {
						
					})
		);

		public void HandleAI((int x, int y) playerPosition, (int x, int y) monsterPosition) {
			SendMessage(MessageId.HandleAI); // Allow for any number of params
		}

		private void StartPatrolling() {
			
		}
	}
}
