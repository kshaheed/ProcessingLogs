
using LiteDB;
using Newtonsoft.Json;
using ProcessLogTask.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace ProcessLogTask
{
	public class ProcessingLogs : IProcessingLogs
	{

		#region Members
		static BlockingCollection<Log> mainList = new BlockingCollection<Log>();
		static BlockingCollection<Target> dbTargets = new BlockingCollection<Target>();
		#endregion

		#region Functions
		public async Task ProcessFileAsync(string filePath)
		{
			//I have tried MemoryMappedFile but took longer so I thought about to make it CQRS better

			using (Task t1 = Task.Run(() =>
			{
				using (var reader = File.OpenText(filePath))
				{
					string currentLine;
					var line = 1;

					#region Update UI with lines count
					ReadingLinesCount();
					#endregion

					var total = File.ReadLines(filePath).Count();

					while ((currentLine = reader.ReadLine()) != null)
					{
						//adding to BlockingCollection
						FillListData(currentLine);

						#region Update UI with reading progress
						Task readingProgressTask = Task.Run(() =>
						{
							Console.WriteLine(new StringBuilder().AppendFormat("Line # {0} / {1}", line, total));
						});
						readingProgressTask.Wait();
						#endregion

						line++;
					}

				}
				mainList.CompleteAdding();
			}))
			{
				// Spin up a Task to consume the BlockingCollection mainList
				try
				{
					using (var db = new LiteDatabase("Logs.db"))
					{
						var col = db.GetCollection<Target>("target");
						// Consume consume the BlockingCollection
						while (true)
						{
							//run multiple tasks
							for (int i = 0; i < 100; i++)
							{
								await Task.Run(() =>
								{
									Consumer(col);
								});
							}
						}


					}
				}
				catch (InvalidOperationException)
				{
					Console.WriteLine("That's All!");
				}
			}
		}

		private static void ReadingLinesCount()
		{
			Task readinglinesCount = Task.Run(() =>
			{
				Console.WriteLine(new StringBuilder().AppendFormat("Reading lines count"));
			});
			readinglinesCount.Wait();
		}

		public void Consumer(LiteCollection<Target> col)
		{
			mainList.TryTake(out Log item);

			if (item == null)
			{
				return;
			}
			
			var duration = 0;
			//IdentifyObjects start and finish objects
			Log startedLog, finishedLog;
			IdentifyObjects(item, out startedLog, out finishedLog);

			if (startedLog != null && finishedLog != null)
			{

				#region Cast timestamp and calculate duration
				if (Double.TryParse(startedLog.Timestamp.ToString(), out double startTimestamp) &&
								Double.TryParse(finishedLog.Timestamp.ToString(), out double endTimestamp))
				{
					duration = CalculatDuration(startTimestamp, endTimestamp);
					if (duration > 4)
					{
						item.Alert = true;
					}
				}
				#endregion

				//Add object To DB
				AddToDB(col, item, duration);

				#region Update UI with adding to DB
				Task addingToDBTask = Task.Run(() =>
					{
						Console.WriteLine(new StringBuilder().AppendFormat("Adding Id # {0} to Database Thread {1}", item.Id,
							Thread.CurrentThread.ManagedThreadId));
					});
				addingToDBTask.Wait();
				#endregion
			}
		}

		private static void IdentifyObjects(Log item, out Log startedLog, out Log finishedLog)
		{
			startedLog = mainList.Where(x => x.Id == item.Id && x.State == "STARTED").FirstOrDefault();
			finishedLog = mainList.Where(x => x.Id == item.Id && x.State == "FINISHED").FirstOrDefault();
			if (item.State == State.FINISHED.ToString())
			{
				finishedLog = item;
			}
			if (item.State == State.STARTED.ToString())
			{
				startedLog = item;
			}
		}

		public bool AddToDB(LiteCollection<Target> col, Log item, int duration)
		{
			var res = false;
			try
			{

				col.Insert(new Target { Id = item.Id, Alert = item.Alert, Duration = duration, Host = item.Host, Type = item.Type });

				//if (dbTargets.Count() == 500 || stopedReading)
				//{
				//	var val = col.InsertBulk(dbTargets);
				//	dbTargets = new BlockingCollection<Target>();
				//}
				res = true;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception while adding to DB " + ex.Message);
			}

			return res;
		}

		public  void  DeleteDBFileAsync()
		{
			try
			{
				// Check if file exists with its full path    
				if (System.IO.File.Exists("Logs.db"))
				{
					// If file found, delete it    
					System.IO.File.Delete("Logs.db");
				}
				else Console.WriteLine("File not found");
				}
			catch (IOException ioExp)
			{
				Console.WriteLine(ioExp.Message);
			}
		}
		#endregion

		#region Helpers
		public int CalculatDuration(double startTimestamp, double endTimestamp)
		{
			int duration = (int)(startTimestamp - endTimestamp);
			//To get positive duration
			duration = duration < 0 ? duration * -1 : duration;
			return duration;
		}
		public void FillListData(string line)
		{
			if (!string.IsNullOrEmpty(line))
			{
				var logObj = JsonConvert.DeserializeObject<Log>(line.ToString());

				if (logObj != null && !string.IsNullOrEmpty(logObj.Id))
				{

					mainList.Add(logObj);
				}
			}
		}
		#endregion



	}

}