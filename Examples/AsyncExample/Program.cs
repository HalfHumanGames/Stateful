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
				AddState("First").
					OnEnterAsync(async (machine) => {
						Console.WriteLine("Getting data.");
						string data = await GetData();
						Console.WriteLine($"Data: {data}");
						await machine.SetTriggerAsync("Done");
					}).
					GoTo("Second").WhenTrigger("Done").
				AddState("Second").
					OnEnterAsync(async (machine) => {
						Console.WriteLine("Getting data.");
						string data = await GetData();
						Console.WriteLine($"Data: {data}");
						// TODO: Add Stop conditions
						//Console.WriteLine("Setting trigger: Done");
						//await machine.SetTriggerAsync("Done");
					}).
				Build.As<StateMachine>();

			stateMachine.LogFlow.Value = true;
			await stateMachine.StartAsync();
			Console.WriteLine($"Active state: {stateMachine.ActiveStateId}");
			Console.ReadLine();
		}

		private static async Task<string> GetData() {
			string url = "https://httpbin.org/get";
			HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url.ToString());
			HttpWebResponse response = (HttpWebResponse) await request.GetResponseAsync();
			Stream stream = response.GetResponseStream();
			StreamReader reader = new StreamReader(stream);
			string data = reader.ReadToEnd();
			reader.Close();
			stream.Close();
			response.Close();
			return data;
		}
	}
}
