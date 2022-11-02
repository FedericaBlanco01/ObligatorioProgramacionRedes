using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Common // Lo voy a poder usar tanto en el cliente como en el servidor
{
    public class NetworkHelper
    {
        NetworkStream networkStream;
        public NetworkHelper(NetworkStream aNetworkStream) { 
            networkStream = aNetworkStream;
        }
           
        public async void Send(byte[] data)
           
        {
            Console.WriteLine(data.Length);
             await networkStream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);    
        }
        

        public async Task<byte[]> ReceiveAsync(int dataLength)
        {
            Console.WriteLine(dataLength);
            var totalReceived = 0;
            byte[] dataLengthBuffer = new byte[dataLength];
            while (totalReceived < dataLength)
            {
                int recieved = await networkStream.ReadAsync(dataLengthBuffer, totalReceived,
                                             dataLength - totalReceived).ConfigureAwait(false);

                if (recieved == 0) // Se corto la conexion del lado del cliente
                {
                   
                    networkStream.Close();
                    throw new SocketException(); // Tendrian que manejarlo de alguna manera
                }

                totalReceived += recieved;
            }
           
            return dataLengthBuffer;
        }
    }
}
