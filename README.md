# StateMachine.NET
[StateMachine.NET](https://halfhumangames.github.io/StateMachine.NET/) is a powerful finite state machine library for .NET

```cs

// Builder that uses a human readable fluent interface
Builder.
  AddState(StateId.StartScreen, new StartScreen()).
    GoTo(StateId.GameScreen).
      WhenTrigger(ParamId.StartGame).
      
  // Hierarchical state machines
  AddState(StateId.GameScreen, Builder.
    AddState(StateId.GameScreen_Player1Turn, new PlayerTurnScreen(1)).
    
      // Define transitions with one or more conditions
      GoTo(StateId.GameScreen_Player2Turn).
        WhenTrigger(ParamId.EndTurn).
    AddState(StateId.GameScreen_Player2Turn, new PlayerTurnScreen(2)).
      GoTo(StateId.GameScreen_Player1Turn).
        WhenTrigger(ParamId.EndTurn).
    AddState(StateId.GameScreen_GameOver, new GameOverScreen()).
    AddState(StateId.GameScreen_PauseMenu, new PauseMenu()).
    
      // Push and pop states
      Pop.WhenBool(ParamId.IsPaused, x => !x).
     
    // Shared transitions
    From(StateId.GameScreen_Player1Turn, StateId.GameScreen_Player2Turn).
      GoTo(StateId.GameScreen_GameOver).
        WhenInt(ParamId.GameStatus, x => x == (int) GameStatus.Player1Won).
        
        // Define alternative transition conditions
        Or.WhenInt(ParamId.GameStatus, x => x == (int) GameStatus.Player2Won).
        Or.WhenInt(ParamId.GameStatus, x => x == (int) GameStatus.Draw).
      
      // Push and pop states
      Push(StateId.GameScreen_PauseMenu).
        WhenBool(ParamId.IsPaused, x => x);
        
        
    // Global transitions
    FromAny.
      GoTo(StateId.StartScreen).
        WhenTrigger(ParamId.GoToStartScreen).
    Build.As<GameScreen>()
```