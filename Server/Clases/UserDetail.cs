using System;
namespace Server.Clases
{
	public class UserDetail
	{
		public string UserEmail { get; set; }

		public string? ProfilePic { get; set; }

		public string[]? Skills { get; set; }

		public string? Description { get; set; }

		public UserDetail(string UserEmail, string ProfilePic)
		{
			this.UserEmail = UserEmail;
			this.ProfilePic = ProfilePic;
		}

		public UserDetail(string UserEmail, string Description, string[] Skills)
		{
			this.UserEmail = UserEmail;
			this.Description = Description;
			this.Skills = Skills;
		}
	}
}

