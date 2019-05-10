#if TASKS

using System.Threading.Tasks;

namespace Stateful.Utilities {

	public static class TaskUtility {

		public static Task CompletedTask => Task.FromResult(0);
	}
}

#endif
