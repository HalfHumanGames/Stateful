using System;
using System.Diagnostics;

namespace Stateful.Utilities {

	public static class Print {

#if NETSTANDARD1_0
		public enum ConsoleColor { 
			Yellow,
			Red,
			Cyan,
			Green,
			Gray
		}
#endif
		private const ConsoleColor warningColor = ConsoleColor.Yellow;
		private const ConsoleColor errorColor = ConsoleColor.Red;
		private const ConsoleColor infoColor = ConsoleColor.Cyan;
		private const ConsoleColor successColor = ConsoleColor.Green;
		private const ConsoleColor defaultColor = ConsoleColor.Gray;

		public static void Warning(object text) => Line(text, warningColor);
		public static void Error(object text) => Line(text, errorColor);
		public static void Info(object text) => Line(text, infoColor);
		public static void Success(object text) => Line(text, successColor);
		public static void Log(object text) => Line(text, defaultColor);

		private static void Line(object text, ConsoleColor colour = defaultColor) {
#if !NETSTANDARD1_0
			ConsoleColor originalColor = Console.ForegroundColor;
			Console.ForegroundColor = colour;
			Console.WriteLine(text.ToString());
			Console.ForegroundColor = originalColor;
			Trace.WriteLine(text.ToString());
#endif
			Debug.WriteLine(text.ToString());
		}
	}
}
