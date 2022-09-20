using Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{

    static void Main(string[] args)
    {
        Console.WriteLine("Arrancando cliente...");

        Socket socketCliente = new Socket(
                            AddressFamily.InterNetwork,
                            SocketType.Stream,
                            ProtocolType.Tcp);


        var localEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);

        socketCliente.Bind(localEndpoint); // el bind al endpoint local es opcional para el cliente

        var remoteEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 30000);

        // me conecto al server: 

        try // no deberíamos tener un try/catch tan grande
        {

            socketCliente.Connect(remoteEndpoint);
            Console.WriteLine("Me conecte al servidor");
            NetworkHelper networkHelper = new NetworkHelper(socketCliente); // Agrego un NetworkHelper

            bool conectado = true;
            while (conectado)
            {
                Console.WriteLine("Bienvenido al sistema");
                Console.WriteLine("Elegir opcion");
                Console.WriteLine("message - enviar mensaje");
                Console.WriteLine("exit - cerrar programa");

                string opcion = Console.ReadLine();

                switch (opcion)
                {
                    case "message":
                        Console.WriteLine("Ingrese mensaje: ");
                        string mensaje = Console.ReadLine();
                        byte[] mensajeEnByte = Encoding.UTF8.GetBytes(mensaje);
                        // enviar el header
                        Header encabezado = new Header(Protocol.Request,
                            Commands.Message,
                            mensajeEnByte.Length);
                        byte[] headerEnBytes = encabezado.GetBytesFromHeader();
                        networkHelper.Send(headerEnBytes);
                        // enviar un mensaje
                        networkHelper.Send(mensajeEnByte);
                    break;

                    case "exit":
                    conectado = false;
                        break;
                }

            }
            socketCliente.Shutdown(SocketShutdown.Both);
            socketCliente.Close();
        }
        catch (SocketException e)
        {
            Console.WriteLine($"Excepcion: {e.Message}, Codigo: {e.ErrorCode}");
        }
        Console.WriteLine("Cerrando cliente... Envie enter para cerrar consola");
        Console.ReadLine();
    }
}
