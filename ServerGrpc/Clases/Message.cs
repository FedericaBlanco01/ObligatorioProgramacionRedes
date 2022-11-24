using System;
namespace ServerGrpc.Clases
{
    public class Message
    {
        public string FromUser { get; set; }

        public string ToUser { get; set; }

        public string Line { get; set; }

        public Message(string from, string to, string msj)
        {
            this.FromUser = from;
            this.ToUser = to;
            this.Line = msj;
        }
    }
}

