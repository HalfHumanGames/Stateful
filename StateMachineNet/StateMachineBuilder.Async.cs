using System;
using System.Threading.Tasks;

namespace StateMachineNet {

	public partial class StateMachineBuilder<TStateId, TParamId, TMessageId> : IStateMachineBuilderFluentInterface<TStateId, TParamId, TMessageId> {
	
		#region Add handlers

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnAsync(TMessageId id, Func<Task> action) { 
			Build.GeTStateId(statesToAddTransitionsTo[0]).OnAsync(id, (stateMachine, state, arg) => action());
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnAsync(TMessageId id, Func<StateMachine<TStateId, TParamId, TMessageId>, Task> action) { 
			Build.GeTStateId(statesToAddTransitionsTo[0]).OnAsync(id, (stateMachine, state, arg) => action(stateMachine));
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnAsync(TMessageId id, Func<StateMachine<TStateId, TParamId, TMessageId>, State<TStateId, TParamId, TMessageId>, Task> action) { 
			Build.GeTStateId(statesToAddTransitionsTo[0]).OnAsync(id, (stateMachine, state, arg) => action(stateMachine, state));
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnAsync(TMessageId id, StateMachine<TStateId, TParamId, TMessageId>.OnMessageAsyncHandler action) { 
			Build.GeTStateId(statesToAddTransitionsTo[0]).OnAsync(id, action);
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnEnterAsync(Func<Task> action) { 
			Build.GeTStateId(statesToAddTransitionsTo[0]).OnEnterAsync((stateMachine, state) => action());
			return this;
		}
		
		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnEnterAsync(Func<StateMachine<TStateId, TParamId, TMessageId>, Task> action) { 
			Build.GeTStateId(statesToAddTransitionsTo[0]).OnEnterAsync((stateMachine, state) => action(stateMachine));
			return this;
		}
		
		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnEnterAsync(StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler action) { 
			Build.GeTStateId(statesToAddTransitionsTo[0]).OnEnterAsync(action);
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnExitAsync(Func<Task> action) { 
			Build.GeTStateId(statesToAddTransitionsTo[0]).OnExitAsync((stateMachine, state) => action());
			return this;
		}
		
		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnExitAsync(Func<StateMachine<TStateId, TParamId, TMessageId>, Task> action) { 
			Build.GeTStateId(statesToAddTransitionsTo[0]).OnExitAsync((stateMachine, state) => action(stateMachine));
			return this;
		}
		
		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnExitAsync(StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler action) { 
			Build.GeTStateId(statesToAddTransitionsTo[0]).OnExitAsync(action);
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnPauseAsync(Func<Task> action) { 
			Build.GeTStateId(statesToAddTransitionsTo[0]).OnPauseAsync((stateMachine, state) => action());
			return this;
		}		
		
		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnPauseAsync(Func<StateMachine<TStateId, TParamId, TMessageId>, Task> action) { 
			Build.GeTStateId(statesToAddTransitionsTo[0]).OnPauseAsync((stateMachine, state) => action(stateMachine));
			return this;
		}		
		
		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnPauseAsync(StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler action) { 
			Build.GeTStateId(statesToAddTransitionsTo[0]).OnPauseAsync(action);
			return this;
		}

		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnResumeAsync(Func<Task> action) { 
			Build.GeTStateId(statesToAddTransitionsTo[0]).OnResumeAsync((stateMachine, state) => action());
			return this;
		}
	
		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnResumeAsync(Func<StateMachine<TStateId, TParamId, TMessageId>, Task> action) { 
			Build.GeTStateId(statesToAddTransitionsTo[0]).OnResumeAsync((stateMachine, state) => action(stateMachine));
			return this;
		}
	
		public IAddHandlerAddTransitionAddStateBuild<TStateId, TParamId, TMessageId> OnResumeAsync(StateMachine<TStateId, TParamId, TMessageId>.OnTransitionAsyncHandler action) { 
			Build.GeTStateId(statesToAddTransitionsTo[0]).OnResumeAsync(action);
			return this;
		}

		#endregion
	}
}
