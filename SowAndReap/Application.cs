using System;

namespace SowAndReap
{
	class Application
	{
		static void Main(string[] arguments)
		{
			if (arguments.Length != 3)
			{
				Console.WriteLine("Usage:");
				Console.WriteLine("<.ardour input path> <.rpp input path> <.rpp output path>");
				return;
			}
			string ardourInputPath = arguments[0];
			string reaperInputPath = arguments[1];
			string reaperOutputPath = arguments[2];
			var parser = new Parser();
			parser.ReadArdourSession(ardourInputPath);
			parser.WriteReaperProject(reaperInputPath, reaperOutputPath);
		}
	}
}
