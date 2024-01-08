using System;
using System.IO;
using Stateful.Utilities;
using Xunit;

namespace Stateful.Tests {

	public enum StateId {
		StateA,
		StateB,
		StateC,
		StateD,
		StateE
	}

	public enum ParamId {
		BoolA,
		BoolB,
		BoolC,
		IntA,
		IntB,
		IntC,
		FloatA,
		FloatB,
		FloatC,
		StringA,
		StringB,
		StringC,
		TriggerA,
		TriggerB,
		TriggerC
	}

	public enum MessageId {
		Message,
		MessageWithParameter,
		MessageWithReturnValue,
		MessageWithParameterAndReturnValue
	}

	public class StatefulTests {

		private IAddStateAddSetParam<StateId, ParamId, MessageId> builder => StateMachineBuilder.Create<StateId, ParamId, MessageId>();

		[Fact]
		public void IsRunnngWorks() {

			var stateMachine = builder.
				AddState(StateId.StateA).
				Build();

			Assert.False(stateMachine.IsRunning);

			stateMachine.Start();
			Assert.True(stateMachine.IsRunning);

			stateMachine.Stop();
			Assert.False(stateMachine.IsRunning);
		}

		[Fact]
		public void IsRunnngWorksForSubstateMachine() {

			var substateMachine = builder.
				AddState(StateId.StateB).
				Build();

			var stateMachine = builder.
				AddState(StateId.StateA, substateMachine).
				Build();

			Assert.False(substateMachine.IsRunning);

			stateMachine.Start();
			Assert.True(substateMachine.IsRunning);

			stateMachine.Stop();
			Assert.False(substateMachine.IsRunning);
		}

		[Fact]
		public void LogFlowWorks() {

			using (StringWriter stringWriter = new StringWriter()) {

				Console.SetOut(stringWriter);

				var stateMachine = builder.
					AddState(StateId.StateA).
					AddState(StateId.StateB).
					Build();

				stateMachine.Start();
				Assert.True(string.IsNullOrEmpty(stringWriter.ToString()));

				stateMachine.LogFlow = true;
				stateMachine.GoTo(StateId.StateB);
				Assert.False(string.IsNullOrEmpty(stringWriter.ToString()));
			}
		}

		[Fact]
		public void LogFlowWorksForSubstateMachine() {

			using (StringWriter stringWriter = new StringWriter()) {

				Console.SetOut(stringWriter);

				var substateMachine = builder.
					AddState(StateId.StateB).
					AddState(StateId.StateC).
					Build();

				var stateMachine = builder.
					AddState(StateId.StateA, substateMachine).
					Build();

				stateMachine.Start();
				Assert.True(string.IsNullOrEmpty(stringWriter.ToString()));

				stateMachine.LogFlow = true;
				stateMachine.GoTo(StateId.StateC);
				Assert.False(string.IsNullOrEmpty(stringWriter.ToString()));
			}
		}

		[Fact]
		public void ParentStateWorks() {

			var substateMachine = builder.
				AddState(StateId.StateB).
				Build();

			var stateMachine = builder.
				AddState(StateId.StateA, substateMachine).
				Build();

			Assert.Null(stateMachine.ParentState);

			Assert.Equal(substateMachine.ParentState, stateMachine);
		}

		[Fact]
		public void ActiveStateIdWorks() {

			var stateMachine = builder.
				AddState(StateId.StateA).
				AddState(StateId.StateB).
				Build();

			Assert.Throws<InvalidOperationException>(() => stateMachine.ActiveStateId);

			stateMachine.Start();
			Assert.Equal(StateId.StateA, stateMachine.ActiveStateId);

			stateMachine.GoTo(StateId.StateB);
			Assert.Equal(StateId.StateB, stateMachine.ActiveStateId);

			stateMachine.Stop();
			Assert.Throws<InvalidOperationException>(() => stateMachine.ActiveStateId);
		}

		[Fact]
		public void ActiveStateWorks() {

			var stateA = new State<StateId, ParamId, MessageId>();
			var stateB = new State<StateId, ParamId, MessageId>();

			var stateMachine = builder.
				AddState(StateId.StateA, stateA).
				AddState(StateId.StateB, stateB).
				Build();

			Assert.Null(stateMachine.ActiveState);

			stateMachine.Start();
			Assert.Equal(stateA, stateMachine.ActiveState);

			stateMachine.GoTo(StateId.StateB);
			Assert.Equal(stateB, stateMachine.ActiveState);

			stateMachine.Stop();
			Assert.Null(stateMachine.ActiveState);
		}

		[Fact]
		public void StartWorks() {

			var stateMachine = builder.
				AddState(StateId.StateA).
				AddState(StateId.StateB).
				Build();

			stateMachine.Start();
			Assert.Equal(StateId.StateA, stateMachine.ActiveStateId);

			Assert.Throws<InvalidOperationException>(() => stateMachine.Start());

			stateMachine.Stop();
			Assert.Throws<ArgumentException>(() => stateMachine.Start(StateId.StateC));

			stateMachine.Start(StateId.StateB);
			Assert.Equal(StateId.StateB, stateMachine.ActiveStateId);
		}

		[Fact]
		public void StopWorks() {

			var stateMachine = builder.
				AddState(StateId.StateA).
				Build();

			Assert.Throws<InvalidOperationException>(() => stateMachine.Stop());

			stateMachine.Start();
			stateMachine.Stop();
			Assert.Null(stateMachine.ActiveState);
		}

		[Fact]
		public void GoToWorks() {

			var substateMachine = builder.
				AddState(StateId.StateC).
				Build();

			var stateMachine = builder.
				AddState(StateId.StateA).
				AddState(StateId.StateB, substateMachine).
				Build();

			Assert.Throws<InvalidOperationException>(() => stateMachine.GoTo(StateId.StateB));

			stateMachine.Start();

			stateMachine.GoTo(StateId.StateB);
			Assert.Equal(StateId.StateB, stateMachine.ActiveStateId);
			Assert.Equal(StateId.StateC, substateMachine.ActiveStateId);

			stateMachine.GoTo(StateId.StateA);
			Assert.Equal(StateId.StateA, stateMachine.ActiveStateId);

			Assert.Throws<ArgumentException>(() => stateMachine.GoTo(StateId.StateD));
		}

		[Fact]
		public void PushWorks() {

			var substateMachine = builder.
				AddState(StateId.StateC).
				Build();

			var stateMachine = builder.
				AddState(StateId.StateA).
				AddState(StateId.StateB, substateMachine).
				AddState(StateId.StateD).
				Build();

			Assert.Throws<InvalidOperationException>(() => stateMachine.Push(StateId.StateB));

			stateMachine.Start();

			stateMachine.Push(StateId.StateB);
			Assert.Equal(2, stateMachine.ActiveStatesCount);
			Assert.Equal(StateId.StateB, stateMachine.ActiveStateId);
			Assert.Equal(StateId.StateC, substateMachine.ActiveStateId);

			Assert.Throws<InvalidOperationException>(() => stateMachine.Push(StateId.StateB));
			Assert.Throws<ArgumentException>(() => stateMachine.Push(StateId.StateE));

			stateMachine.Push(StateId.StateD);
			Assert.Equal(3, stateMachine.ActiveStatesCount);
			Assert.Equal(StateId.StateD, stateMachine.ActiveStateId);
			Assert.Equal(StateId.StateC, substateMachine.ActiveStateId);
		}

		[Fact]
		public void PopWorks() {

			var substateMachine = builder.
				AddState(StateId.StateD).
				Build();

			var stateMachine = builder.
				AddState(StateId.StateA).
				AddState(StateId.StateB).
				AddState(StateId.StateC, substateMachine).
				Build();

			Assert.Throws<InvalidOperationException>(() => stateMachine.Pop());

			stateMachine.Start();
			stateMachine.Push(StateId.StateB);
			stateMachine.Push(StateId.StateC);
			Assert.Equal(3, stateMachine.ActiveStatesCount);
			Assert.Equal(StateId.StateC, stateMachine.ActiveStateId);

			stateMachine.Pop();
			Assert.Equal(2, stateMachine.ActiveStatesCount);
			Assert.Equal(StateId.StateB, stateMachine.ActiveStateId);

			stateMachine.Pop();
			Assert.Equal(1, stateMachine.ActiveStatesCount);
			Assert.Equal(StateId.StateA, stateMachine.ActiveStateId);

			stateMachine.Pop();
			Assert.Equal(0, stateMachine.ActiveStatesCount);
			Assert.Throws<InvalidOperationException>(() => stateMachine.ActiveStateId);

			Assert.Throws<InvalidOperationException>(() => stateMachine.Pop());
		}

		[Fact]
		public void WhenBoolWorks() {
			
			var stateMachine = builder.
				AddState(StateId.StateA).
					GoTo(StateId.StateB).
						WhenBool(ParamId.BoolA, x => x).
						WhenBool(ParamId.BoolB, x => !x).
				AddState(StateId.StateB).
					GoTo(StateId.StateA).
						WhenBool(ParamId.BoolA, x => !x).
						WhenBool(ParamId.BoolB, x => x).
				Build();


			stateMachine.Start();
			Assert.Equal(StateId.StateA, stateMachine.ActiveStateId);

			stateMachine.SetBool(ParamId.BoolA, true);
			Assert.Equal(StateId.StateB, stateMachine.ActiveStateId);

			stateMachine.SetBool(ParamId.BoolB, true);
			Assert.Equal(StateId.StateB, stateMachine.ActiveStateId);

			stateMachine.SetBool(ParamId.BoolA, false);
			Assert.Equal(StateId.StateA, stateMachine.ActiveStateId);
		}

		[Fact]
		public void WhenFloatWorks() {

			var stateMachine = builder.
				AddState(StateId.StateA).
					GoTo(StateId.StateB).
						WhenFloat(ParamId.FloatA, x => x == 1.27f).
						WhenFloat(ParamId.FloatB, x => x == 0).
				AddState(StateId.StateB).
					GoTo(StateId.StateA).
						WhenFloat(ParamId.FloatA, x => x < 0).
						WhenFloat(ParamId.FloatB, x => x > 1.75f).
				Build();

			stateMachine.Start();
			Assert.Equal(StateId.StateA, stateMachine.ActiveStateId);

			stateMachine.SetFloat(ParamId.FloatA, 1.271f);
			Assert.Equal(StateId.StateA, stateMachine.ActiveStateId);

			stateMachine.SetFloat(ParamId.FloatA, 1.27f);
			Assert.Equal(StateId.StateB, stateMachine.ActiveStateId);

			stateMachine.SetFloat(ParamId.FloatB, 1.75f);
			Assert.Equal(StateId.StateB, stateMachine.ActiveStateId);

			stateMachine.SetFloat(ParamId.FloatA, -1);
			Assert.Equal(StateId.StateB, stateMachine.ActiveStateId);

			stateMachine.SetFloat(ParamId.FloatB, 1.751f);
			Assert.Equal(StateId.StateA, stateMachine.ActiveStateId);
		}

		[Fact]
		public void WhenIntWorks() {

			var stateMachine = builder.
				AddState(StateId.StateA).
					GoTo(StateId.StateB).
						WhenInt(ParamId.IntA, x => x == 3).
						WhenInt(ParamId.IntB, x => x == 1).
				AddState(StateId.StateB).
					GoTo(StateId.StateA).
						WhenInt(ParamId.IntA, x => x < 0).
						WhenInt(ParamId.IntB, x => x > 3).
				Build();

			stateMachine.Start();
			Assert.Equal(StateId.StateA, stateMachine.ActiveStateId);

			stateMachine.SetInt(ParamId.IntA, 3);
			Assert.Equal(StateId.StateA, stateMachine.ActiveStateId);

			stateMachine.SetInt(ParamId.IntB, 1);
			Assert.Equal(StateId.StateB, stateMachine.ActiveStateId);

			stateMachine.SetInt(ParamId.IntB, 4);
			Assert.Equal(StateId.StateB, stateMachine.ActiveStateId);

			stateMachine.SetInt(ParamId.IntA, -1);
			Assert.Equal(StateId.StateA, stateMachine.ActiveStateId);
		}

		[Fact]
		public void WhenStringWorks() {

			var stateMachine = builder.
				AddState(StateId.StateA).
					GoTo(StateId.StateB).
						WhenString(ParamId.StringA, x => x == "Foo").
						WhenString(ParamId.StringB, x => x == "Bar").
				AddState(StateId.StateB).
					GoTo(StateId.StateA).
						WhenString(ParamId.StringA, x => x.CompareTo("Foo") > 0).
						WhenString(ParamId.StringB, x => string.IsNullOrEmpty(x)).
				Build();

			stateMachine.Start();
			Assert.Equal(StateId.StateA, stateMachine.ActiveStateId);

			stateMachine.SetString(ParamId.StringA, "Foo");
			Assert.Equal(StateId.StateA, stateMachine.ActiveStateId);

			stateMachine.SetString(ParamId.StringB, "Bar");
			Assert.Equal(StateId.StateB, stateMachine.ActiveStateId);

			stateMachine.SetString(ParamId.StringA, "Goo");
			Assert.Equal(StateId.StateB, stateMachine.ActiveStateId);

			stateMachine.SetString(ParamId.StringB, null);
			Assert.Equal(StateId.StateA, stateMachine.ActiveStateId);
		}

		[Fact]
		public void WhenWorks() {

			var observableInt = new Observable<int>(0);
			var observableBool = new Observable<bool>(false);

			var stateMachine = builder.
				AddState(StateId.StateA).
					GoTo(StateId.StateB).
						When(observableInt, x => x == 3).
						When(observableBool, x => x).
				AddState(StateId.StateB).
					GoTo(StateId.StateA).
						When(observableInt, x => x < 0).
						When(observableBool, x => !x).
				Build();

			stateMachine.Start();
			Assert.Equal(StateId.StateA, stateMachine.ActiveStateId);

			observableInt.Value = 3;
			Assert.Equal(StateId.StateA, stateMachine.ActiveStateId);

			observableBool.Value = true;
			Assert.Equal(StateId.StateB, stateMachine.ActiveStateId);

			observableInt.Value = -1;
			Assert.Equal(StateId.StateB, stateMachine.ActiveStateId);

			observableBool.Value = false;
			Assert.Equal(StateId.StateA, stateMachine.ActiveStateId);
		}

		[Fact]
		public void GetAndSetBoolWorks() { 

			var stateMachine = builder.
				AddState(StateId.StateA).
				Build();

			Assert.False(stateMachine.GetBool(ParamId.BoolA));
			Assert.Throws<InvalidOperationException>(() => stateMachine.SetBool(ParamId.BoolA, true));

			stateMachine.Start();

			Assert.False(stateMachine.GetBool(ParamId.BoolA));
			Assert.False(stateMachine.GetBool(ParamId.BoolB));

			stateMachine.SetBool(ParamId.BoolA, false);
			Assert.False(stateMachine.GetBool(ParamId.BoolA));

			stateMachine.SetBool(ParamId.BoolB, true);
			Assert.True(stateMachine.GetBool(ParamId.BoolB));

			stateMachine.SetBool(ParamId.BoolA, true);
			Assert.True(stateMachine.GetBool(ParamId.BoolA));

			stateMachine.SetBool(ParamId.BoolB, false);
			Assert.False(stateMachine.GetBool(ParamId.BoolB));

			stateMachine.Stop();

			Assert.True(stateMachine.GetBool(ParamId.BoolA));
			Assert.Throws<InvalidOperationException>(() => stateMachine.SetBool(ParamId.BoolB, true));
		}

		[Fact]
		public void GetAndSetFloatWorks() {

			var stateMachine = builder.
				AddState(StateId.StateA).
				Build();

			Assert.Equal(0, stateMachine.GetFloat(ParamId.FloatA));
			Assert.Throws<InvalidOperationException>(() => stateMachine.SetFloat(ParamId.FloatA, 1.5f));

			stateMachine.Start();

			Assert.Equal(0, stateMachine.GetFloat(ParamId.FloatA));
			Assert.Equal(0, stateMachine.GetFloat(ParamId.FloatB));

			stateMachine.SetFloat(ParamId.FloatA, 1.5f);
			Assert.Equal(1.5f, stateMachine.GetFloat(ParamId.FloatA));

			stateMachine.SetFloat(ParamId.FloatB, -2.25f);
			Assert.Equal(-2.25f, stateMachine.GetFloat(ParamId.FloatB));

			stateMachine.SetFloat(ParamId.FloatA, .85f, true);
			Assert.Equal(2.35f, stateMachine.GetFloat(ParamId.FloatA));

			stateMachine.SetFloat(ParamId.FloatB, -.5f, true);
			Assert.Equal(-2.75f, stateMachine.GetFloat(ParamId.FloatB));

			stateMachine.SetFloat(ParamId.FloatA, -4.99f);
			Assert.Equal(-4.99f, stateMachine.GetFloat(ParamId.FloatA));

			stateMachine.SetFloat(ParamId.FloatB, 3.05f);
			Assert.Equal(3.05f, stateMachine.GetFloat(ParamId.FloatB));

			stateMachine.Stop();

			Assert.Equal(-4.99f, stateMachine.GetFloat(ParamId.FloatA));
			Assert.Throws<InvalidOperationException>(() => stateMachine.SetFloat(ParamId.FloatB, 2.75f));
		}


		[Fact]
		public void GetAndSetIntWorks() {

			var stateMachine = builder.
				AddState(StateId.StateA).
				Build();

			Assert.Equal(0, stateMachine.GetInt(ParamId.IntA));
			Assert.Throws<InvalidOperationException>(() => stateMachine.SetInt(ParamId.IntA, 1));

			stateMachine.Start();

			Assert.Equal(0, stateMachine.GetInt(ParamId.IntA));
			Assert.Equal(0, stateMachine.GetInt(ParamId.IntB));

			stateMachine.SetInt(ParamId.IntA, 1);
			Assert.Equal(1, stateMachine.GetInt(ParamId.IntA));

			stateMachine.SetInt(ParamId.IntB, -2);
			Assert.Equal(-2, stateMachine.GetInt(ParamId.IntB));

			stateMachine.SetInt(ParamId.IntA, 13, true);
			Assert.Equal(14, stateMachine.GetInt(ParamId.IntA));

			stateMachine.SetInt(ParamId.IntB, -5, true);
			Assert.Equal(-7, stateMachine.GetInt(ParamId.IntB));

			stateMachine.SetInt(ParamId.IntA, -5);
			Assert.Equal(-5, stateMachine.GetInt(ParamId.IntA));

			stateMachine.SetInt(ParamId.IntB, 3);
			Assert.Equal(3, stateMachine.GetInt(ParamId.IntB));

			stateMachine.Stop();

			Assert.Equal(-5, stateMachine.GetInt(ParamId.IntA));
			Assert.Throws<InvalidOperationException>(() => stateMachine.SetInt(ParamId.IntB, 3));
		}


		[Fact]
		public void GetAndSetStringWorks() {

			var stateMachine = builder.
				AddState(StateId.StateA).
				Build();

			Assert.Null(stateMachine.GetString(ParamId.StringA));
			Assert.Throws<InvalidOperationException>(() => stateMachine.SetString(ParamId.StringA, "Foo"));

			stateMachine.Start();

			Assert.Null(stateMachine.GetString(ParamId.StringA));
			Assert.Null(stateMachine.GetString(ParamId.StringB));

			stateMachine.SetString(ParamId.StringA, "Foo");
			Assert.Equal("Foo", stateMachine.GetString(ParamId.StringA));

			stateMachine.SetString(ParamId.StringB, "Bar");
			Assert.Equal("Bar", stateMachine.GetString(ParamId.StringB));

			stateMachine.SetString(ParamId.StringA, "tball", true);
			Assert.Equal("Football", stateMachine.GetString(ParamId.StringA));

			stateMachine.SetString(ParamId.StringB, "celona", true);
			Assert.Equal("Barcelona", stateMachine.GetString(ParamId.StringB));

			stateMachine.SetString(ParamId.StringA, "Goo");
			Assert.Equal("Goo", stateMachine.GetString(ParamId.StringA));

			stateMachine.SetString(ParamId.StringB, "Tiki");
			Assert.Equal("Tiki", stateMachine.GetString(ParamId.StringB));

			stateMachine.Stop();

			Assert.Equal("Goo", stateMachine.GetString(ParamId.StringA));
			Assert.Throws<InvalidOperationException>(() => stateMachine.SetString(ParamId.StringB, "Tiki"));
		}

		[Fact]
		public void GetStateWorks() {

			var stateA = new State<StateId, ParamId, MessageId>();
			var stateB = new State<StateId, ParamId, MessageId>();

			var stateMachine = builder.
				AddState(StateId.StateA, stateA).
				AddState(StateId.StateB, stateB).
				Build();

			Assert.Equal(stateA, stateMachine.GetState(StateId.StateA));
			Assert.Equal(stateB, stateMachine.GetState(StateId.StateB));

			Assert.Throws<ArgumentException>(() => stateMachine.GetState(StateId.StateC));
		}

		[Fact]
		public void SendMessageWorks() {

			string log = "";

			var stateMachine = builder.
				AddState(StateId.StateA).
					// Without brackets, setting log also implies return log, so we
					// add brackets to clarify which messages have no return value.
					On(MessageId.Message, () => {
						log = "StateA -> Message";
					}).On(MessageId.MessageWithParameter, (machine, state, arg) => {
						log = "StateA -> MessageWithParameter: " + arg;
					}).On(MessageId.MessageWithReturnValue, () =>
						"StateA -> MessageWithReturnValue"
					).On(MessageId.MessageWithParameterAndReturnValue, (machine, state, arg) =>
						"StateA -> MessageWithParameterAndReturnValue: " + arg
					).
				AddState(StateId.StateB).
					On(MessageId.Message, () => {
						log = "StateB -> Message";
					}).On(MessageId.MessageWithParameter, (machine, state, arg) => {
						log = "StateB -> MessageWithParameter: " + arg;
					}).On(MessageId.MessageWithReturnValue, () =>
						"StateB -> MessageWithReturnValue"
					).On(MessageId.MessageWithParameterAndReturnValue, (machine, state, arg) =>
						"StateB -> MessageWithParameterAndReturnValue: " + arg
					).
				Build();

			Assert.Throws<InvalidOperationException>(() => 
				stateMachine.SendMessage(MessageId.Message)
			);

			stateMachine.Start();
			stateMachine.SendMessage(MessageId.Message);
			Assert.Equal("StateA -> Message", log);
			
			stateMachine.SendMessage(MessageId.MessageWithParameter, "Foo");
			Assert.Equal("StateA -> MessageWithParameter: Foo", log);

			log = stateMachine.SendMessage<string>(MessageId.MessageWithReturnValue);
			Assert.Equal("StateA -> MessageWithReturnValue", log);

			log = stateMachine.SendMessage<string>(MessageId.MessageWithParameterAndReturnValue, "Bar");
			Assert.Equal("StateA -> MessageWithParameterAndReturnValue: Bar", log);

			stateMachine.GoTo(StateId.StateB);
			stateMachine.SendMessage(MessageId.Message);
			Assert.Equal("StateB -> Message", log);
			
			stateMachine.SendMessage(MessageId.MessageWithParameter, "Foo");
			Assert.Equal("StateB -> MessageWithParameter: Foo", log);

			log = stateMachine.SendMessage<string>(MessageId.MessageWithReturnValue);
			Assert.Equal("StateB -> MessageWithReturnValue", log);

			log = stateMachine.SendMessage<string>(MessageId.MessageWithParameterAndReturnValue, "Bar");
			Assert.Equal("StateB -> MessageWithParameterAndReturnValue: Bar", log);

			Assert.Throws<ArgumentException>(() => stateMachine.SendMessage<string>(MessageId.Message));
			Assert.Throws<ArgumentException>(() => stateMachine.SendMessage(MessageId.MessageWithReturnValue));
			stateMachine.Pop();
			Assert.Throws<InvalidOperationException>(() => stateMachine.SendMessage(MessageId.Message));

		}
	}
}
