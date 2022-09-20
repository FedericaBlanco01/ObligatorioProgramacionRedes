using System;
using System.Net.Cache;

namespace Common
{
    public class Protocol
    {
        //public const int LargoFijo = 4; // Largo del largo del mensaje

        public const string Request = "REQ";
        public const string Response = "RES";

        public const int CommandLength = 2;
        public const int DataLengthLength = 4;
    }
}
