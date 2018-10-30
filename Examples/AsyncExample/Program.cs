using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using StateMachineNet;

namespace AsyncExample {

	public class Program {

		private static void Main(string[] args) => MainAsync(args).Wait();

		private static async Task MainAsync(string[] args) {
			StateMachine stateMachine = StateMachineBuilder.Create().
				AddState("Start").
					OnAsync("Get", async (machine, state, url) => {
						HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url.ToString());
						HttpWebResponse response = (HttpWebResponse) await request.GetResponseAsync();
						Stream stream = response.GetResponseStream();
						StreamReader reader = new StreamReader(stream);
						string data = reader.ReadToEnd();
						Console.WriteLine(data);
						reader.Close();
						stream.Close();
						response.Close();
						machine.SetTrigger("Stop");
					}).
					GoTo("Stop").WhenTrigger("Stop").
				AddState("Stop").
					OnEnter(() => Console.WriteLine("Done!")).
				Build.As<StateMachine>();

			stateMachine.Start();
			Console.WriteLine("Getting data async!");
			await stateMachine.SendMessageAsync("Get", "https://httpbin.org/get");
			Console.WriteLine("Done getting data async!");
			Console.WriteLine($"Current state: {stateMachine.ActiveStateId}");
			Console.ReadLine(); // What is the state of the state machine here?
		}
	}
}
