using Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Program
{

    static void Main(string[] args)
    {
        Console.WriteLine("Creando Socket Server");

        Socket server = new Socket(
                            AddressFamily.InterNetwork,
                            SocketType.Stream,
                            ProtocolType.Tcp);

        var localEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 30000);

        server.Bind(localEndpoint);
        int backlog = 3;
        server.Listen(backlog);

        while (true)
        {
            Socket cliente = server.Accept(); // Es bloqueante
            Thread manejarCliente = new Thread(() => HandleClient(cliente));
            manejarCliente.Start();
        }

    }

    static void HandleClient(Socket cliente)
    {
        // Acepte un cliente y estoy conectado 
        Console.WriteLine("Acepte un nuevo cliente");

        NetworkHelper networkHelper = new NetworkHelper(cliente);
        bool conectado = true;
        while (conectado)
        {
            try
            {

                // Recibimos el header
                ///.....
                Header encabezado = new Header();

                byte[] encabezadoEnBytes =
                    networkHelper.Receive(Protocol.Request.Length + Protocol.CommandLength + Protocol.DataLengthLength);
                encabezado.DecodeHeader(encabezadoEnBytes);

                switch (encabezado.comando)
                {
                    case Commands.Message:
                        // recibe un mensaje
                        byte [] mensajeEnBytes = networkHelper.Receive(encabezado.largoDeDatos);
                        string mensaje = Encoding.UTF8.GetString(mensajeEnBytes);
                        Console.WriteLine($"El usuario dice: {mensaje}");
                        break;
                    case Commands.Login:
                        Console.WriteLine("No esta implentado");
                        break;
                    case Commands.ListUsers:
                        Console.WriteLine("No implementado");
                        break;
                }



                // Agarro el comando y realizo la accion para ese comando
            }
            catch (SocketException)
            {
                Console.WriteLine("Se desconecto el cliente");
                conectado = false;
            }
        }
        Console.WriteLine("Cerrando conexión con cliente...");
        cliente.Shutdown(SocketShutdown.Both);
        cliente.Close();
    }

}


