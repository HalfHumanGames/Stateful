using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using StateMachineNet;
using StateMachineNet.Utilities;

namespace AsyncExample {

	public class Program {

		private static void Main(string[] args) => MainAsync(args).Wait();

		private static async Task MainAsync(string[] args) {

			StateMachine stateMachine = StateMachineBuilder.Create().
				AddState("First").
					OnEnterAsync(async (machine) => {
						Print.Log("Getting data.");
						string data = await GetData();
						Print.Log($"Data: {data}");
						await machine.SetTriggerAsync("Done");
					}).
					GoTo("Second").WhenTrigger("Done").
				AddState("Second").
					OnEnterAsync(async (machine) => {
						Print.Log("Getting data.");
						string data = await GetData();
						Print.Log($"Data: {data}");
						// TODO: Add Stop conditions
						//Print.Log("Setting trigger: Done");
						//await machine.SetTriggerAsync("Done");
					}).
				Build.As<StateMachine>();

			stateMachine.LogFlow.Value = true;
			await stateMachine.StartAsync();
			Print.Log($"Active state: {stateMachine.ActiveStateId}");
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
