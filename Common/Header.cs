using System;
using System.Text;

namespace Common
{
    public class Header
    {


        // pasar de direccion, comando, largo y data a byte

        private byte[] directionInBytes;
        private byte[] commandInBytes;
        private byte[] dataLengthInBytes;
        public Header(string direccion, int comando, int largoData)
        {
            directionInBytes = Encoding.UTF8.GetBytes(direccion);

            string commandInString = comando.ToString("D2"); // convierto de 5 a "05"
            commandInBytes = Encoding.UTF8.GetBytes(commandInString);

            string largoDeDataInString = largoData.ToString("D4"); // convierto de 5 a "0005"
            // En clase en vez de decir "dataLenghtInBytes = ...", decia "commandInBytes = ..." por eso fallaba♂
            dataLengthInBytes = Encoding.UTF8.GetBytes(largoDeDataInString);

        }

        public byte[] GetBytesFromHeader()
        {
            // devuelve la array con los bytes del header
            byte[] encabezadoEnBytes = new byte[Protocol.Request.Length + Protocol.CommandLength + Protocol.DataLengthLength];

            Array.Copy(this.directionInBytes, 0, encabezadoEnBytes, 0, Protocol.Request.Length);
            Array.Copy(this.commandInBytes, 0, encabezadoEnBytes, Protocol.Request.Length, Protocol.CommandLength);
            Array.Copy(this.dataLengthInBytes, 0, encabezadoEnBytes, Protocol.Request.Length + Protocol.CommandLength, Protocol.DataLengthLength);
            return encabezadoEnBytes;
        }

        /// Recepcion del header:
        ///

        public Header()
        {
        }

        public string direccion { get; private set; }
        public int comando { get; private set; }

        public int largoDeDatos { get; private set; }

        public void DecodeHeader(byte[] encabezadoEnBytes)
        {
            this.direccion = Encoding.UTF8.GetString(encabezadoEnBytes, 0, Protocol.Request.Length);

            string comandoInString =
                Encoding.UTF8.GetString(encabezadoEnBytes, Protocol.Request.Length, Protocol.CommandLength);
            this.comando = int.Parse(comandoInString);  // try/catch 

            string largoDeDatosInString = Encoding.UTF8.GetString(encabezadoEnBytes,
                Protocol.Request.Length + Protocol.CommandLength, Protocol.DataLengthLength);
            this.largoDeDatos = int.Parse(largoDeDatosInString);

        }
    }
}
