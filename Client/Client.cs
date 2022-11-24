using Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Communication;
using System.Configuration;
using System.IO;

class Program
{


    static async Task Main(string[] args)
    {
        Console.WriteLine("Arrancando cliente...");

        try
        {
            Common.SettingsManager.SetupConfiguration(ConfigurationManager.AppSettings);
            var clientIpEndPoint = new IPEndPoint(
            IPAddress.Parse(SettingsManager.IpClient),
            int.Parse(SettingsManager.PortClient));
            var tcpClient = new TcpClient(clientIpEndPoint);
            var keepConnection = true;


            while (keepConnection)
            {
                var word = string.Empty;
                while (string.IsNullOrEmpty(word) || string.IsNullOrWhiteSpace(word))
                {
                    Console.WriteLine("Bienvenido al sistema");
                    Console.WriteLine("SI- conectarse al servidor");
                    Console.WriteLine("NO- conectarse al servidor");
                    word = Console.ReadLine();
                }
                switch (word)
                {
                    case "SI":
                        await tcpClient.ConnectAsync(
                        IPAddress.Parse(SettingsManager.IpServer),
                        int.Parse(SettingsManager.PortServer)).ConfigureAwait(false);
                        Console.WriteLine("Conexión con servidor exitosa" + SettingsManager.IpServer + SettingsManager.PortServer);
                        await using (var networkStream = tcpClient.GetStream())
                        {
                            NetworkHelper networkHelper = new NetworkHelper(networkStream);
                            await HandleConnectionMenu(networkHelper);
                            tcpClient.Close();
                        }
                        break;
                    case "NO":
                        Console.WriteLine("Chau... :(");
                        keepConnection = false;
                        break;
                    default:
                        Console.WriteLine("Comando inexistente");
                        break;
                }
            }
        }
        catch (IOException)
        {
            Console.WriteLine("Error de conexion con el servidor");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }


        Console.WriteLine("Cerrando cliente...");
        Console.ReadLine();
    }

    public static async Task Register(NetworkHelper networkHelper)
    {
        try
        {
            Console.WriteLine("Ingrese su nombre: ");
            var newName = Console.ReadLine();

            Console.WriteLine("Ingrese su mail: ");
            var newEmail = Console.ReadLine();

            Console.WriteLine("Ingrese su constrasena: ");
            var newPassword = Console.ReadLine();

            string mensaje = newName + "/" + newEmail + "/" + newPassword;

            byte[] mensajeEnByte = Encoding.UTF8.GetBytes(mensaje);

            // enviar el header
            Header encabezado = new Header(Common.Protocol.Request,
                Commands.Register,
                mensajeEnByte.Length);

            byte[] encabezadoEnBytes = encabezado.GetBytesFromHeader();
            await networkHelper.Send(encabezadoEnBytes);

            await networkHelper.Send(mensajeEnByte);
            //end

            //recibo
            Header encabezadoRecibo = new Header();

            byte[] encabezadoRecibidoEnBytes =
                await networkHelper.ReceiveAsync(Common.Protocol.Request.Length + Common.Protocol.CommandLength + Common.Protocol.DataLengthLength);
            encabezadoRecibo.DecodeHeader(encabezadoRecibidoEnBytes);

            byte[] registerEnBytes = await networkHelper.ReceiveAsync(encabezadoRecibo.largoDeDatos);
            string responseCodificado = Encoding.UTF8.GetString(registerEnBytes);
            Console.WriteLine(responseCodificado);
            if (responseCodificado.Equals("Usuario registrado exitosamente"))
            {
                await HandleLoggedMenu(networkHelper);
            }
        }
        catch (Exception e)
        {
            throw (e);
        }
    }

    public static async Task Login(NetworkHelper networkHelper)
    {
        try
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
            await networkHelper.Send(headerEnBytes);

            await networkHelper.Send(dataEnBytes);
            //end

            //recibo

            Header encabezadoRecibo = new Header();

            byte[] encabezadoRecibidoEnBytes =
                await networkHelper.ReceiveAsync(Common.Protocol.Request.Length + Common.Protocol.CommandLength + Common.Protocol.DataLengthLength);
            encabezadoRecibo.DecodeHeader(encabezadoRecibidoEnBytes);

            byte[] registerEnBytes = await networkHelper.ReceiveAsync(encabezadoRecibo.largoDeDatos);
            string responseCodificado = Encoding.UTF8.GetString(registerEnBytes);
            Console.WriteLine(responseCodificado);
            if (responseCodificado.Equals("Se inició sesion correctamente"))
            {
                await HandleLoggedMenu(networkHelper);
            }
        }
        catch (Exception e)
        {
            throw (e);
        }

    }

    public static async Task HandleConnectionMenu(NetworkHelper networkHelper)
    {
        try
        {
            bool conectado = true;
            while (conectado)
            {
                Console.WriteLine("Inicio");
                Console.WriteLine("1- regístrese");
                Console.WriteLine("2- iniciar sesión");
                Console.WriteLine("EXIT - cerrar programa");

                var opcion = Console.ReadLine();
                switch (opcion)
                {
                    case "1":
                        await Register(networkHelper);
                        break;

                    case "2":
                        await Login(networkHelper);
                        break;

                    case "EXIT":
                        conectado = false;
                        break;

                    default:
                        Console.WriteLine("Comando inexistente");
                        break;
                }
            }
        }
        catch (Exception e)
        {
            throw (e);
        }
    }

    public static async Task CrearPerfilLaboral(NetworkHelper networkHelper)
    {
        try
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
            await networkHelper.Send(headerEnBytes);

            await networkHelper.Send(dataEnBytes);
            //end

            //recibo

            Header encabezadoRecibo = new Header();

            byte[] encabezadoRecibidoEnBytes =
                await networkHelper.ReceiveAsync(Common.Protocol.Request.Length + Common.Protocol.CommandLength + Common.Protocol.DataLengthLength);
            encabezadoRecibo.DecodeHeader(encabezadoRecibidoEnBytes);

            byte[] registerEnBytes = await networkHelper.ReceiveAsync(encabezadoRecibo.largoDeDatos);
            string responseCodificado = Encoding.UTF8.GetString(registerEnBytes);
            Console.WriteLine(responseCodificado);
        }
        catch (Exception e)
        {
            throw (e);
        }

    }

    public static async Task FotoPerfil(NetworkHelper networkHelper)
    {
        try
        {
            Console.WriteLine("Ingrese la ruta completa al archivo: ");
            String abspath = Console.ReadLine();

            byte[] dataEnBytes = Encoding.UTF8.GetBytes("Envio de foto");

            // envio header and length


            //envio file a server
            var fileCommonHandler = new FileCommsHandler(networkHelper);
            bool fileExists = fileCommonHandler.ValidatePath(abspath);

            ////

            string mensajeFile = "No";

            if (fileExists)
            {
                mensajeFile = "Si";
            }
            byte[] mensajeFileEnByte = Encoding.UTF8.GetBytes(mensajeFile);

            // enviar el header
            Header encabezado = new Header(Common.Protocol.Request,
                Commands.ProfilePic,
                mensajeFileEnByte.Length);

            byte[] encabezadoEnBytes = encabezado.GetBytesFromHeader();
            await networkHelper.Send(encabezadoEnBytes);

            await networkHelper.Send(mensajeFileEnByte);

            if (fileExists)
            {
                await fileCommonHandler.SendFile(abspath);
            }

            //recibo

            Header encabezadoRecibo = new Header();

            byte[] encabezadoRecibidoEnBytes =
               await networkHelper.ReceiveAsync(Common.Protocol.Request.Length + Common.Protocol.CommandLength + Common.Protocol.DataLengthLength);
            encabezadoRecibo.DecodeHeader(encabezadoRecibidoEnBytes);

            byte[] registerEnBytes = await networkHelper.ReceiveAsync(encabezadoRecibo.largoDeDatos);
            string responseCodificado = Encoding.UTF8.GetString(registerEnBytes);
            Console.WriteLine(responseCodificado);
        }
        catch (Exception e)
        {
            throw (e);
        }
    }

    public static async Task BuscadorDeUsuarios(NetworkHelper networkHelper)
    {
        try
        {
            Console.WriteLine("Ingrese las habilidades o palabras a buscar: ");
            string data = Console.ReadLine();

            byte[] dataEnBytes = Encoding.UTF8.GetBytes(data);

            // enviar 
            Header header = new Header(Common.Protocol.Request,
                Commands.ListUsers,
                dataEnBytes.Length);

            byte[] headerEnBytes = header.GetBytesFromHeader();
            await networkHelper.Send(headerEnBytes);

            await networkHelper.Send(dataEnBytes);

            //recibo

            Header encabezadoRecibo = new Header();

            byte[] encabezadoRecibidoEnBytes =
                await networkHelper.ReceiveAsync(Common.Protocol.Request.Length + Common.Protocol.CommandLength + Common.Protocol.DataLengthLength);
            encabezadoRecibo.DecodeHeader(encabezadoRecibidoEnBytes);

            byte[] registerEnBytes = await networkHelper.ReceiveAsync(encabezadoRecibo.largoDeDatos);
            string responseCodificado = Encoding.UTF8.GetString(registerEnBytes);
            Console.WriteLine(responseCodificado);
        }
        catch (Exception e)
        {
            throw (e);
        }

    }

    public static async Task BuscadorUsuarioEspecífico(NetworkHelper networkHelper)
    {
        try
        {
            Console.WriteLine("Ingrese el Email del usuario a buscar: ");
            string data = Console.ReadLine();

            byte[] dataEnBytes = Encoding.UTF8.GetBytes(data);

            // enviar 
            Header header = new Header(Common.Protocol.Request,
                Commands.ListSpecificUser,
                dataEnBytes.Length);

            byte[] headerEnBytes = header.GetBytesFromHeader();
            await networkHelper.Send(headerEnBytes);

            await networkHelper.Send(dataEnBytes);

            //recibo si va a haber foto
            Header encabezadoAvisoFoto = new Header();

            byte[] encabezadoFotoRecibidoEnBytes =
                await networkHelper.ReceiveAsync(Common.Protocol.Request.Length + Common.Protocol.CommandLength + Common.Protocol.DataLengthLength);
            encabezadoAvisoFoto.DecodeHeader(encabezadoFotoRecibidoEnBytes);

            byte[] registerFotoEnBytes = await networkHelper.ReceiveAsync(encabezadoAvisoFoto.largoDeDatos);
            string responseFotoCodificado = Encoding.UTF8.GetString(registerFotoEnBytes);
            if (responseFotoCodificado.Equals("Si"))
            {
                //recibo de img
                var fileCommonHandler = new FileCommsHandler(networkHelper);
                var fileName = fileCommonHandler.ReceiveFileAsync();
            }
            //recibo

            Header encabezadoRecibo = new Header();

            byte[] encabezadoRecibidoEnBytes =
                await networkHelper.ReceiveAsync(Common.Protocol.Request.Length + Common.Protocol.CommandLength + Common.Protocol.DataLengthLength);
            encabezadoRecibo.DecodeHeader(encabezadoRecibidoEnBytes);

            byte[] registerEnBytes = await networkHelper.ReceiveAsync(encabezadoRecibo.largoDeDatos);
            string responseCodificado = Encoding.UTF8.GetString(registerEnBytes);
            Console.WriteLine(responseCodificado);
        }
        catch (Exception e)
        {
            throw (e);
        }

    }

    public static async Task LeerMensajesChat(NetworkHelper networkHelper)
    {
        try
        {
            Console.WriteLine("Ingrese el mail del usuario con el que quiere leer chat");
            string otroUsuario = Console.ReadLine();
            string data = otroUsuario;

            byte[] dataEnBytes = Encoding.UTF8.GetBytes(data);

            // enviar 
            Header header = new Header(Common.Protocol.Request,
                Commands.ReadChat,
                dataEnBytes.Length);

            byte[] headerEnBytes = header.GetBytesFromHeader();
            await networkHelper.Send(headerEnBytes);

            await networkHelper.Send(dataEnBytes);

            //recibo

            Header encabezadoRecibo = new Header();

            byte[] encabezadoRecibidoEnBytes =
                await networkHelper.ReceiveAsync(Common.Protocol.Request.Length + Common.Protocol.CommandLength + Common.Protocol.DataLengthLength);
            encabezadoRecibo.DecodeHeader(encabezadoRecibidoEnBytes);

            byte[] registerEnBytes = await networkHelper.ReceiveAsync(encabezadoRecibo.largoDeDatos);
            string responseCodificado = Encoding.UTF8.GetString(registerEnBytes);
            Console.WriteLine(responseCodificado);

        }
        catch (Exception e)
        {
            throw (e);
        }

    }

    public static async Task EnviarMensajeChat(NetworkHelper networkHelper)
    {
        try
        {
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
            await networkHelper.Send(headerEnBytes);

            await networkHelper.Send(dataEnBytes);

            Console.WriteLine("mensaje enviado correctamente");
        }
        catch (Exception e)
        {
            throw (e);
        }

    }

    public static async Task ChatMenu(NetworkHelper networkHelper)
    {
        try
        {
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
                        await LeerMensajesChat(networkHelper);
                        break;

                    case "2":
                        await EnviarMensajeChat(networkHelper);
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
        catch (Exception e)
        {
            throw (e);
        }
    }

    public static async Task HandleLoggedMenu(NetworkHelper networkHelper)
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
                        await CrearPerfilLaboral(networkHelper);
                        break;

                    case "2":
                        await FotoPerfil(networkHelper);
                        break;

                    case "3":
                        await BuscadorDeUsuarios(networkHelper);
                        break;

                    case "4":
                        await ChatMenu(networkHelper);
                        break;

                    case "5":
                        await BuscadorUsuarioEspecífico(networkHelper);
                        break;

                    case "6":
                        conectado = false; //poder desconectar al cliente del servidor
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
    }
}
