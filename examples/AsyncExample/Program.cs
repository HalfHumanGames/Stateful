using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Stateful;
using Stateful.Utilities;

namespace AsyncExample {

	public class Program {

		private static void Main(string[] args) {

			var stateMachine = StateMachineBuilder.Create().
				AddState("Enter").
					OnEnterAsync(async (machine) => {
						Print.Log("Getting data.");
						string data = await GetData();
						Print.Log($"Data: {data}");
					}).
				Build();

			stateMachine.LogFlow = true;
			stateMachine.StartAsync().ContinueWith(x => Print.Log("Done."));
			Print.Log($"First hello");
			Thread.Sleep(1000);
			Print.Log($"Second hello");
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
