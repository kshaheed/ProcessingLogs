using System;
using System.Threading.Tasks;

namespace ProcessLogTask
{
	class Program
	{
		static async Task Main(string[] args)
		{
			ProcessingLogs processingLogs = new ProcessingLogs();
			Console.WriteLine("Please enter task.txt file path");
			var fileName= Console.ReadLine();
			if (string.IsNullOrEmpty(fileName))
			{
				Console.WriteLine("File name is empty");
			}

			await processingLogs.ProcessFile(fileName);

			Console.WriteLine("Press any key to exit.");
			Console.ReadKey();
		}
	}
}
