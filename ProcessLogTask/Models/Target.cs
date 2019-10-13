using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessLogTask.Models
{
	public class Target
	{
		public string Id { get; set; }
		public double Duration { get; set; }
		public string Type { get; set; }
		public string Host { get; set; }
		public bool Alert { get; set; }
	}
}
