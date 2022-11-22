using System;
namespace NuevorServidor.Clases
{
    public class UserDetail
    {
        public string UserEmail { get; set; }

        public string Skills { get; set; }

        public string Description { get; set; }

        public string PhotoName { get; set; }

        public UserDetail(string UserEmail, string Description, string Skills)
        {
            this.UserEmail = UserEmail;
            this.Description = Description;
            this.Skills = Skills;
            this.PhotoName = "";
        }
    }
}

