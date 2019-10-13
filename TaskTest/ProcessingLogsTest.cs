using LiteDB;
using NUnit.Framework;
using ProcessLogTask;
using ProcessLogTask.Models;
using System.Threading.Tasks;

namespace Tests
{
	public class ProcessingLogsTest
	{
		[SetUp]
		public void Setup()
		{
		}

		[TestCase(5, 1491377495218, 1491377495213)]
		[TestCase(5, 1491377495213, 1491377495218)]
		public async Task TestDurationCalculationAsync(int expected, double start, double end)
		{
			//Arrange
			ProcessingLogs processingLogs = new ProcessingLogs();
			//Act
			var res = processingLogs.CalculatDuration(start, end);
			//Assert
			Assert.AreEqual(expected, res);
		}

		[TestCase(true, "scsmbstgrb")]
		[TestCase(false, "scsmbstgrb")]
		public async Task AddObjToDBAsync(bool expected, string id)
		{
			//Arrange
			ProcessingLogs processingLogs = new ProcessingLogs();
			bool res = false;
			//Act
			using (var db = new LiteDatabase("mydb.db"))
			{
				var col = db.GetCollection<Target>("target");
				res = processingLogs.AddToDB(col, new Log { Id = id, State = "STARTED", Timestamp = 1491377495213 }, 5);
			}
			//Assert
			Assert.AreEqual(expected, res);
		}

	}
}