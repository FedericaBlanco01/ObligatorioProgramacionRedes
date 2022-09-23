using System;
using System.Collections.Generic;

namespace Server.Clases
{
	public class Singleton
	{
		public List<User> Users { get; set; }

		public List<UserDetail> UserDetails { get; set; }

		public List<Log> Chats { get; set; }

		private readonly object LockUsers = new object();

		private readonly object LockUsersDetails = new object();

		public Singleton()
		{
			this.Users = new List<User>();
			this.UserDetails = new List<UserDetail>();
			this.Chats = new List<Log>();
		}


		public bool ValidateData(string email)
		{
			lock (LockUsers)
			{
				foreach (User user in Users)
				{
					if (user.Email.Equals(email))
					{
						return false;
					}
				}
				return true;
			}
		}

		public void AddUser(User user)
		{
			lock (LockUsers)
			{
				Users.Add(user);
			}
		}

		public void AddDetail(UserDetail detail)
		{
			lock (LockUsers)
			{
				lock (LockUsersDetails)
				{
					UserDetails.Add(detail);
				}
			}
		}

		public User LoginBack(string email, string password)  
		{
			lock (LockUsers)
			{
				foreach (User user in Users)
				{
					if (user.Email.Equals(email) && user.Password.Equals(password))
					{
						return user;
					}
				}
				return null;
			}
		}
	}
}

