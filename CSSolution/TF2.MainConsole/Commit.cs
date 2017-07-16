using System;

namespace TF2.MainConsole
{
	internal class Commit
	{
		public Int32 Position { get; set; }
		public String Hash { get; set; }

		private String branch;
		public String Branch
		{
			get { return branch; }
			set { branch = String.IsNullOrEmpty(value) ? Git.DEFAULT_BRANCH : value; }
		}

		public String Tag { get; set; }
		public String Author { get; set; }
		public DateTime DateTime { get; set; }
		public String Message { get; set; }


		public override string ToString()
		{
			return $"{Position}:{Hash}";
		}
	}
}