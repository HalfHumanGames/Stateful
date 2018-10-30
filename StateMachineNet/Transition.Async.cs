using System.Threading.Tasks;

namespace StateMachineNet {

	internal abstract partial class Transition<TStateId, TParamId, TMessageId> {

		internal abstract Task DoTransitionAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine);
	}

	internal partial class GoToTransition<TStateId, TParamId, TMessageId> : Transition<TStateId, TParamId, TMessageId> {

		internal override async Task DoTransitionAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) =>
			await stateMachine.GoToAsync(State);
	}

	internal partial class PushTransition<TStateId, TParamId, TMessageId> : Transition<TStateId, TParamId, TMessageId> {

		internal override async Task DoTransitionAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) =>
			await stateMachine.PushAsync(State);
	}

	internal partial class PopTransition<TStateId, TParamId, TMessageId> : Transition<TStateId, TParamId, TMessageId> {

		internal override async Task DoTransitionAsync(StateMachine<TStateId, TParamId, TMessageId> stateMachine) =>
			await stateMachine.PopAsync();
	}
}
