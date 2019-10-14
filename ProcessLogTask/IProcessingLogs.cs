using LiteDB;
using ProcessLogTask.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ProcessLogTask
{
	public interface IProcessingLogs
	{

		int CalculatDuration(double startTimestamp, double endTimestamp);
		void Consumer(LiteCollection<Target> col);
		void FillListData(string line);
		Task ProcessFileAsync(string filePath);
		bool AddToDB(LiteCollection<Target> col, Log item, int duration);
		void DeleteDBFileAsync();
	}
}
