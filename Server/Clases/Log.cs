using System;
using System.Collections.Generic;

namespace Server.Clases
{
	public class Log
	{
		public string User1 { get; set; }

		public string User2 { get; set; }

		public List<Message> Message { get; set; }

		public Log(string User1, string User2)
		{
			this.User1 = User1;
			this.User2 = User2;
			this.Message = new List<Message>();
		}
	}
}

