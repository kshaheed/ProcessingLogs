using System;
using System.Threading.Tasks;

namespace ProcessLogTask
{
	class Program
	{
		static async Task Main(string[] args)
		{
			ProcessingLogs processingLogs = new ProcessingLogs();
			var fileName= Console.ReadLine();
			await processingLogs.ProcessFile(fileName);

			Console.WriteLine("Press any key to exit.");
			Console.ReadKey();
		}
	}
}
