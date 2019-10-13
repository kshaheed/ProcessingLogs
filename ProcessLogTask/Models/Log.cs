using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessLogTask.Models
{
	public class Log
	{
		public string Id { get; set; }
		public string State { get; set; }
		public string Type { get; set; }
		public string Host { get; set; }
		public long Timestamp { get; set; }
		public bool Alert { get; set; }
	}
}
