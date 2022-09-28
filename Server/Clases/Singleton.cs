using System;
using System.Collections.Generic;

namespace Server.Clases
{
	public class Singleton
	{
		public List<User> Users { get; set; }

		public List<UserDetail> UserDetails { get; set; }

		public List<Log> Chats { get; set; }

		private readonly object LockChats = new object();

		private readonly object LockUsers = new object();

		private readonly object LockUsersDetails = new object();

		public Singleton()
		{
			this.Users = new List<User>();
			this.UserDetails = new List<UserDetail>();
			this.Chats = new List<Log>();
		}

		public bool CheckForUserDetail(string UserEmail){
			lock (LockUsersDetails)
			{
				foreach (UserDetail userDetail in UserDetails)
				{
					if (userDetail.UserEmail.Equals(UserEmail))
					{
						return true;
					}
				}
				return false;
			}
		}

        public string LeerChat(string user1, string user2)
        {
            string ret = "";
            lock (LockChats)
            {
                foreach (Log log in Chats)
                {
                    if ((user1.Equals(log.User1) && user2.Equals(log.User2)) || (user2.Equals(log.User1) && user1.Equals(log.User2))) 
                    {
                        foreach (Message message in log.Message)
                        {

                            ret += message.FromUser + " : " + message.Line + "\n";

                        }
                    }
                }
                return ret;
            }
        }

        public void EnviarChat(string userFrom, string userTo, string chatLine)
        {
			
            lock (LockChats)
            {
                foreach (Log log in Chats)
                {
					if ((userFrom.Equals(log.User1) && userTo.Equals(log.User2)) || (userTo.Equals(log.User1) && userFrom.Equals(log.User2)))
					{
						log.Message.Add(new Message(userFrom,userTo,chatLine));
                        return;
                    }
					
                }

				Log newLog = new Log(userFrom, userTo);
				newLog.Message.Add(new Message(userFrom, userTo, chatLine));
				Chats.Add(newLog);
            }	
        }

		public void SetUserFotoName(User user, string fotoName)
		{
			lock (LockUsersDetails)
			{
				foreach (UserDetail userDetail in UserDetails)
				{
					if (userDetail.UserEmail.Equals(user.Email))
					{
						userDetail.PhotoName = fotoName;
						return;
					}
				}
			}
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

		public UserDetail SpecificUserProfile(string usuarioABuscar)
        {

			UserDetail ret = null;

			lock (LockUsers)
			{
				foreach (User user in Users)
				{
					if (user.Name.Equals(usuarioABuscar))
					{
						lock (LockUsersDetails)
						{
							foreach (UserDetail userD in UserDetails)
							{
								if (user.Email.Equals(userD.UserEmail))
								{
									ret = userD;
								}
							}
						}
					}
				}
                return ret;
            }
            
        }

        public List<UserDetail> UsersWithCoincidences(string palabraABuscar) {

			List<UserDetail> ret = new List<UserDetail>();

			lock (LockUsersDetails)
			{
				foreach (UserDetail userD in UserDetails)
				{
					if (userD.Skills.Contains(palabraABuscar) || userD.Description.Contains(palabraABuscar))
					{
						ret.Add(userD);

					}		
				}
                return ret;
            }
			
		}

	}
}

