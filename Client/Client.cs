using Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Communication;
using System.Configuration;

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

                Common.SettingsManager.SetupConfiguration(ConfigurationManager.AppSettings);

                var localEndpoint = new IPEndPoint(IPAddress.Parse(Common.SettingsManager.IpClient), Int32.Parse(Common.SettingsManager.PortClient));

                socketCliente.Bind(localEndpoint);

                var remoteEndpoint = new IPEndPoint(IPAddress.Parse(Common.SettingsManager.IpServer), Int32.Parse(Common.SettingsManager.PortServer));
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
        catch (Exception e)
        {
            Console.WriteLine($"Excepcion: {e.Message}");
        }

        Console.WriteLine("Cerrando cliente...");
        Console.ReadLine();
    }

    public static void Register(NetworkHelper networkHelper, Socket socketCliente)
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
            if (responseCodificado.Equals("Usuario registrado exitosamente"))
            {
                HandleLoggedMenu(networkHelper, socketCliente);
            }
        }
        catch (Exception e)
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
            if (responseCodificado.Equals("Se inició sesion correctamente"))
            {       
                HandleLoggedMenu(networkHelper, socketCliente);
            }
        }
        catch (Exception e)
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
            Console.WriteLine("1- regístrese");
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

    public static void BuscadorDeUsuarios(NetworkHelper networkHelper) {

        Console.WriteLine("Ingrese las habilidades o palabras a buscar: ");
        string data = Console.ReadLine();

        byte[] dataEnBytes = Encoding.UTF8.GetBytes(data);

        // enviar 
        Header header = new Header(Common.Protocol.Request,
            Commands.ListUsers,
            dataEnBytes.Length);

        byte[] headerEnBytes = header.GetBytesFromHeader();
        networkHelper.Send(headerEnBytes);

        networkHelper.Send(dataEnBytes);

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

    public static void BuscadorUsuarioEspecífico(NetworkHelper networkHelper, Socket cliente)
    {

        Console.WriteLine("Ingrese el nombre del usuario a buscar: ");
        string data = Console.ReadLine();

        byte[] dataEnBytes = Encoding.UTF8.GetBytes(data);

        // enviar 
        Header header = new Header(Common.Protocol.Request,
            Commands.ListSpecificUser,
            dataEnBytes.Length);

        byte[] headerEnBytes = header.GetBytesFromHeader();
        networkHelper.Send(headerEnBytes);

        networkHelper.Send(dataEnBytes);

        //recibo de img
        var fileCommonHandler = new FileCommsHandler(cliente);
        var fileName = fileCommonHandler.ReceiveFile();

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

    public static void LeerMensajesChat(NetworkHelper networkHelper) {

        Console.WriteLine("Ingrese el mail del usuario con el que quiere leer chat");
        string otroUsuario = Console.ReadLine();
        string data = otroUsuario;

        byte[] dataEnBytes = Encoding.UTF8.GetBytes(data);

        // enviar 
        Header header = new Header(Common.Protocol.Request,
            Commands.ReadChat,
            dataEnBytes.Length);

        byte[] headerEnBytes = header.GetBytesFromHeader();
        networkHelper.Send(headerEnBytes);

        networkHelper.Send(dataEnBytes);

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

    public static void EnviarMensajeChat(NetworkHelper networkHelper) {

        Console.WriteLine("Ingrese el mail del usuario al que le quiere enviar un mensaje");
        string otroUsuario = Console.ReadLine();
        Console.WriteLine("Ingrese el  mensaje");
        string mensaje = Console.ReadLine();
        string data = otroUsuario + "/" + mensaje;

        byte[] dataEnBytes = Encoding.UTF8.GetBytes(data);

        // enviar 
        Header header = new Header(Common.Protocol.Request,
            Commands.SendChat,
            dataEnBytes.Length);

        byte[] headerEnBytes = header.GetBytesFromHeader();
        networkHelper.Send(headerEnBytes);

        networkHelper.Send(dataEnBytes);

        Console.WriteLine("mensaje enviado correctamente");

    }

    public static void ChatMenu(NetworkHelper networkHelper) {

        bool conectado = true;

        while (conectado)
        {
            Console.WriteLine("1- Leer mensajes recibidos");
            Console.WriteLine("2- Enviar mensaje");
            Console.WriteLine("3- Salir");
            string opcion = Console.ReadLine();

            switch (opcion)
            {
                case "1":
                    LeerMensajesChat(networkHelper);
                    break;

                case "2":
                    EnviarMensajeChat(networkHelper);
                    break;

                case "3":
                    conectado = false;
                    break;

                default:
                    Console.WriteLine("Comando inexistente");
                    break;
            }
        }
    }

    public static void HandleLoggedMenu(NetworkHelper networkHelper, Socket socketCliente)
    {
        bool conectado = true;
        while (conectado)
        {
            Console.WriteLine("Bienvenido linkedin");
            Console.WriteLine("1- cree su perfil laboral");
            Console.WriteLine("2- suba su foto de perfil");
            Console.WriteLine("3- consultar perfiles existentes");
            Console.WriteLine("4- chat");
            Console.WriteLine("5- consultar un perfil específico y descargar su foto de perfil");
            Console.WriteLine("6- exit");

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
                        BuscadorDeUsuarios(networkHelper);
                        break;

                    case "4":
                        ChatMenu(networkHelper);
                        break;

                    case "5":
                        BuscadorUsuarioEspecífico(networkHelper, socketCliente);
                        break;
                        
                    case "6":
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
