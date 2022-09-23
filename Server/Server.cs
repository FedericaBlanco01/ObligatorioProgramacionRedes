using Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Server.Clases;
using System.Collections.Generic;

class Program
{

    static void Main(string[] args)
    {
        Console.WriteLine("Creando Socket Server");

        Singleton singleton = new Singleton();

        Socket server = new Socket(
                            AddressFamily.InterNetwork,
                            SocketType.Stream,
                            ProtocolType.Tcp);

        var localEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7000);

        server.Bind(localEndpoint);
        int backlog = 3;
        server.Listen(backlog);

        while (true)
        {
            Socket cliente = server.Accept();
            NetworkHelper networkHelper = new NetworkHelper(cliente);
            Thread manejarCliente = new Thread(() => HandleClient(cliente, singleton, networkHelper));
            manejarCliente.Start();
        }

    }

    static User Login( NetworkHelper networkHelper, Header encabezado, Singleton system)
    {
        byte[] loginEnBytes = networkHelper.Receive(encabezado.largoDeDatos);
        string loginCodificado = Encoding.UTF8.GetString(loginEnBytes);
        string[] loginData = loginCodificado.Split("/");

        User loggedUser = system.LoginBack(loginData[0], loginData[1]);
        string loggedMessage = "";

        if (loggedUser != null)
        {
            loggedMessage = "logged in successfully";
            Console.WriteLine($"User email: {loginData[0]}");
            Console.WriteLine($"User password: {loginData[1]}");
        }
        else
        {
            loggedMessage = "your email or password are incorrect";
        }

        //envio
        byte[] mensajeLogInEnByte = Encoding.UTF8.GetBytes(loggedMessage);

        Header encabezadoLogInEnvio = new Header(Protocol.Request,
            Commands.Register,
            mensajeLogInEnByte.Length);

        byte[] encabezadoLogInEnvioEnBytes = encabezadoLogInEnvio.GetBytesFromHeader();
        networkHelper.Send(encabezadoLogInEnvioEnBytes);

        networkHelper.Send(mensajeLogInEnByte);
        //end

        return loggedUser;

    }

    static User Register(NetworkHelper networkHelper, Header encabezado, Singleton system)
    {
        // recibe un mensaje
    
        byte[] registerEnBytes = networkHelper.Receive(encabezado.largoDeDatos);
        string registerCodificado = Encoding.UTF8.GetString(registerEnBytes);
        string[] registerData = registerCodificado.Split("/");
        
        string mensaje = "";

        User newUser = null;

        if (system.ValidateData(registerData[1]))
        {
            newUser = new User(registerData[0], registerData[1], registerData[2]);
            system.AddUser(newUser);

            Console.WriteLine($"User created");
            Console.WriteLine($"User name: {registerData[0]}");
            Console.WriteLine($"User email: {registerData[1]}");
            Console.WriteLine($"User password: {registerData[2]}");
            mensaje = "Usuario registrado exitosamente";
        }
        else
        {
            mensaje = "Email ya en uso";
        }

        byte[] mensajeEnByte = Encoding.UTF8.GetBytes(mensaje);

        // enviar el header
        Header encabezadoEnvio = new Header(Protocol.Request,
            Commands.Register,
            mensajeEnByte.Length);

        byte[] encabezadoEnvioEnBytes = encabezadoEnvio.GetBytesFromHeader();
        networkHelper.Send(encabezadoEnvioEnBytes);

        // enviar register info
        networkHelper.Send(mensajeEnByte);

        return newUser;
    }

    static void HandleClient(Socket cliente, Singleton system, NetworkHelper networkHelper)
    {
        // Acepte un cliente y estoy conectado 
        Console.WriteLine("Un nuevo cliente establecio conexión");
        bool conectado = true;
        User user = null;
        while (conectado)
        {
            try
            {
                Header encabezado = new Header();

                byte[] encabezadoEnBytes =
                    networkHelper.Receive(Protocol.Request.Length + Protocol.CommandLength + Protocol.DataLengthLength);
                encabezado.DecodeHeader(encabezadoEnBytes);

                switch (encabezado.comando)
                {
                    case Commands.Register:
                        user = Register(networkHelper, encabezado, system);
                        break;

                    case Commands.Login:
                        user = Login(networkHelper, encabezado, system);
                        break;

                }
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


