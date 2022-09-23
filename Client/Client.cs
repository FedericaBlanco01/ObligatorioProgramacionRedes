﻿using Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Communication;

class Program
{

    static void Main(string[] args)
    {
        Console.WriteLine("Arrancando cliente...");

        try
        {
            while (true)
            {

                Socket socketCliente = new Socket(
                            AddressFamily.InterNetwork,
                            SocketType.Stream,
                            ProtocolType.Tcp);

                var localEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);

                socketCliente.Bind(localEndpoint); 

                var remoteEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7000);
                NetworkHelper networkHelper = new NetworkHelper(socketCliente);

                Console.WriteLine("Bienvenido al sistema");
                Console.WriteLine("SI- conectarse al servidor");
                Console.WriteLine("NO- conectarse al servidor");

                string inicio = Console.ReadLine();

                switch (inicio)
                {
                    case "SI":
                        socketCliente.Connect(remoteEndpoint);
                        Console.WriteLine("Conección con servidor exitosa");

                        HandleConnectionMenu(networkHelper, socketCliente);
                        break;
                    case "NO":
                        Console.WriteLine("Chau... :(");
                        break;
                    default:
                        Console.WriteLine("Comando inexistente");
                        break;
                }
            }
         }
         catch (SocketException e)
        {
            Console.WriteLine($"Excepcion: {e.Message}, Codigo: {e.ErrorCode}");
        }
        catch(Exception e)
        {
            Console.WriteLine($"Excepcion: {e.Message}");
        }

         Console.WriteLine("Cerrando cliente...");
         Console.ReadLine();
    }

    public static void Register( NetworkHelper networkHelper, Socket socketCliente)
    {
        Console.WriteLine("Ingrese su nombre: ");
        string newName = Console.ReadLine();

        Console.WriteLine("Ingrese su mail: ");
        string newEmail = Console.ReadLine();

        Console.WriteLine("Ingrese su constrasena: ");
        string newPassword = Console.ReadLine();

        string mensaje = newName + "/" + newEmail + "/" + newPassword;

        byte[] mensajeEnByte = Encoding.UTF8.GetBytes(mensaje);

        // enviar el header
        Header encabezado = new Header(Common.Protocol.Request,
            Commands.Register,
            mensajeEnByte.Length);

        byte[] encabezadoEnBytes = encabezado.GetBytesFromHeader();
        networkHelper.Send(encabezadoEnBytes);

        networkHelper.Send(mensajeEnByte);
        //end

        //recibo
        Header encabezadoRecibo = new Header();
        try
        {
            byte[] encabezadoRecibidoEnBytes =
                networkHelper.Receive(Common.Protocol.Request.Length + Common.Protocol.CommandLength + Common.Protocol.DataLengthLength);
            encabezadoRecibo.DecodeHeader(encabezadoRecibidoEnBytes);

            byte[] registerEnBytes = networkHelper.Receive(encabezadoRecibo.largoDeDatos);
            string responseCodificado = Encoding.UTF8.GetString(registerEnBytes);
            Console.WriteLine(responseCodificado);
            if(responseCodificado.Equals("Usuario registrado exitosamente"))
            {
                HandleLoggedMenu(networkHelper, socketCliente);
            }
        }
        catch(Exception e)
        {
            throw (e);
        } 
    }

    public static void Login(NetworkHelper networkHelper, Socket socketCliente)
    {
        Console.WriteLine("Ingrese su mail: ");
        string email = Console.ReadLine();

        Console.WriteLine("Ingrese su constraseña: ");
        string password = Console.ReadLine();

        string data = email + "/" + password;

        byte[] dataEnBytes = Encoding.UTF8.GetBytes(data);

        // enviar 
        Header header = new Header(Common.Protocol.Request,
            Commands.Login,
            dataEnBytes.Length);

        byte[] headerEnBytes = header.GetBytesFromHeader();
        networkHelper.Send(headerEnBytes);

        networkHelper.Send(dataEnBytes);
        //end

        //recibo
        try
        {
            Header encabezadoRecibo = new Header();

            byte[] encabezadoRecibidoEnBytes =
                networkHelper.Receive(Common.Protocol.Request.Length + Common.Protocol.CommandLength + Common.Protocol.DataLengthLength);
            encabezadoRecibo.DecodeHeader(encabezadoRecibidoEnBytes);

            byte[] registerEnBytes = networkHelper.Receive(encabezadoRecibo.largoDeDatos);
            string responseCodificado = Encoding.UTF8.GetString(registerEnBytes);
            Console.WriteLine(responseCodificado);
            if (responseCodificado.Equals("logged in successfully"))
            {
                HandleLoggedMenu(networkHelper, socketCliente);
            }
        }
        catch(Exception e)
        {
            throw (e);
        }

    }

    public static void HandleConnectionMenu(NetworkHelper networkHelper, Socket socketCliente)
    {
        bool conectado = true;
        while (conectado)
        {
            Console.WriteLine("Inicio");
            Console.WriteLine("1- registresé");
            Console.WriteLine("2- iniciar sesión");
            Console.WriteLine("EXIT - cerrar programa");

            string opcion = Console.ReadLine();

            try
            {
                switch (opcion)
                {
                    case "1":
                        Register(networkHelper, socketCliente);
                        break;

                    case "2":
                        Login(networkHelper, socketCliente);
                        break;

                    case "EXIT":
                        conectado = false;
                        break;
                    default:
                        Console.WriteLine("Comando inexistente");
                        break;
                }
            }
            catch (Exception e)
            {
                throw (e);
            }

        }
        socketCliente.Shutdown(SocketShutdown.Both);
        socketCliente.Close();
    }

    public static void CrearPerfilLaboral(NetworkHelper networkHelper, Socket socketCliente)
    {
        Console.WriteLine("Ingrese sus habilidades: ");
        string habilidades = Console.ReadLine();

        Console.WriteLine("Ingrese una descripcion: ");
        string desc = Console.ReadLine();

        string data = habilidades + "/" + desc;

        byte[] dataEnBytes = Encoding.UTF8.GetBytes(data);

        // enviar 
        Header header = new Header(Common.Protocol.Request,
            Commands.JobProfile,
            dataEnBytes.Length);

        byte[] headerEnBytes = header.GetBytesFromHeader();
        networkHelper.Send(headerEnBytes);

        networkHelper.Send(dataEnBytes);
        //end

        //recibo
        try
        {
            Header encabezadoRecibo = new Header();

            byte[] encabezadoRecibidoEnBytes =
                networkHelper.Receive(Common.Protocol.Request.Length + Common.Protocol.CommandLength + Common.Protocol.DataLengthLength);
            encabezadoRecibo.DecodeHeader(encabezadoRecibidoEnBytes);

            byte[] registerEnBytes = networkHelper.Receive(encabezadoRecibo.largoDeDatos);
            string responseCodificado = Encoding.UTF8.GetString(registerEnBytes);
            Console.WriteLine(responseCodificado);
        }
        catch (Exception e)
        {
            throw (e);
        }

    }

    public static void FotoPerfil(NetworkHelper networkHelper, Socket socketCliente)
    {
        Console.WriteLine("Ingrese la ruta completa al archivo: ");
        String abspath = Console.ReadLine();

        byte[] dataEnBytes = Encoding.UTF8.GetBytes("Envio de foto");

        // envio header and length
        Header header = new Header(Common.Protocol.Request,
            Commands.ProfilePic,
            dataEnBytes.Length);

        byte[] headerEnBytes = header.GetBytesFromHeader();
        networkHelper.Send(headerEnBytes);

        //envio file a server
        var fileCommonHandler = new FileCommsHandler(socketCliente);
        fileCommonHandler.SendFile(abspath);
    }

    public static void HandleLoggedMenu(NetworkHelper networkHelper, Socket socketCliente)
    {
        bool conectado = true;
        while (conectado)
        {
            Console.WriteLine("Bienvenido linkedin");
            Console.WriteLine("1- cree su perfil laboral");
            Console.WriteLine("2- suba su foto de perfil");
            Console.WriteLine("3- buscador de usuarios");
            Console.WriteLine("4- chat");
            Console.WriteLine("5- exit");

            string opcion = Console.ReadLine();

            try
            {
                switch (opcion)
                {
                    case "1":
                        CrearPerfilLaboral(networkHelper, socketCliente);
                        break;

                    case "2":
                        FotoPerfil(networkHelper, socketCliente);
                        break;

                    case "3":
                        Console.WriteLine("not implemented yet");
                        break;

                    case "4":
                        Console.WriteLine("not implemented yet");
                        break;

                    case "5":
                        conectado = false; //poder desconectar al cliente del servidor
                        break;

                    default:
                        Console.WriteLine("Comando inexistente");
                        break;
                }
            }catch(Exception e)
            {
                throw (e);
            }

        }
    }
}