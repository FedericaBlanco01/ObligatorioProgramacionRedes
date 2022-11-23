using Grpc.Core;
using NuevorServidor;
using Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NuevorServidor.Clases;
using Communication;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace NuevorServidor.Services;

public class GreeterService : Perfil.PerfilBase
{
    private readonly ILogger<GreeterService> _logger;
    public static Singleton _singleton = new Singleton();
    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }

    public override Task<Response> CrearPerfil(PerfilData request, ServerCallContext context)
    {
        UserDetail newDetails = new UserDetail(request.Email, request.Descripcion, request.Habilidades);
        _singleton.AddDetail(newDetails);
        return Task.FromResult(new Response { Message = "Perfil creado" });
    }

    public override Task<Response> EliminarPerfil(PerfilIdentifier request, ServerCallContext context)
    {
        _singleton.DeleteDetail(request.Email);
        return Task.FromResult(new Response { Message = "Perfil eliminado" });
    }

    public override Task<Response> EditarPerfil(PerfilData request, ServerCallContext context)
    {
        _singleton.EditDetail(request.Email, request.Descripcion, request.Habilidades);
        return Task.FromResult(new Response { Message = "Perfil editado" });
    }

    public static bool working = true;
    private static List<TcpClient> clients = new List<TcpClient>();
    public static async Task Main()
    {
        Console.WriteLine("Creando Socket Server");


        Common.SettingsManager.SetupConfiguration(System.Configuration.ConfigurationManager.AppSettings);

        var localEndpoint = new IPEndPoint(IPAddress.Parse(Common.SettingsManager.IpServer), Int32.Parse(Common.SettingsManager.PortServer));
        var tcpListener = new TcpListener(localEndpoint);
        Console.WriteLine(Common.SettingsManager.IpServer + " " + Common.SettingsManager.PortServer);

        tcpListener.Start(3);

        Console.WriteLine("Escriba Exit cuando quiera cerrar el Server");

        while (working)
        {
            var closeTheServer = Task.Run(async () => await closeServer());

            var task = Task.Run(async () => await HandleClient(tcpListener).ConfigureAwait(false));

        }
        Console.WriteLine("Cerrando servidor");
    }



    static async Task closeServer()
    {
        string message = Console.ReadLine();
        if (message.Equals("Exit"))
        {

            foreach (TcpClient client in clients)
            {
                client.GetStream().Close();
                client.Close();
            }
            working = false;

        }
    }

    static async Task<NuevorServidor.Clases.User> LoginAsync(NetworkHelper networkHelper, Header encabezado, NetworkStream networkStream)
    {
        byte[] loginEnBytes = await networkHelper.ReceiveAsync(encabezado.largoDeDatos);
        string loginCodificado = Encoding.UTF8.GetString(loginEnBytes);
        string[] loginData = loginCodificado.Split("/");

        NuevorServidor.Clases.User loggedUser = _singleton.LoginBack(loginData[0], loginData[1]);
        string loggedMessage = "";

        if (loggedUser != null)
        {
            loggedMessage = "Se inició sesion correctamente";
            Console.WriteLine($"User email: {loginData[0]}");
            Console.WriteLine($"User password: {loginData[1]}");
        }
        else
        {
            loggedMessage = "tu email o contraseña son incorrectas";
        }

        //envio
        byte[] mensajeLogInEnByte = Encoding.UTF8.GetBytes(loggedMessage);

        Header encabezadoLogInEnvio = new Header(Common.Protocol.Request,
            Commands.Register,
            mensajeLogInEnByte.Length);

        byte[] encabezadoLogInEnvioEnBytes = encabezadoLogInEnvio.GetBytesFromHeader();
        networkHelper.Send(encabezadoLogInEnvioEnBytes);

        networkHelper.Send(mensajeLogInEnByte);
        //end

        return loggedUser;

    }

    static async Task<NuevorServidor.Clases.User> RegisterAsync(NetworkHelper networkHelper, Header encabezado, NetworkStream networkStream)
    {
        // recibe un mensaje

        byte[] registerEnBytes = await networkHelper.ReceiveAsync(encabezado.largoDeDatos);
        string registerCodificado = Encoding.UTF8.GetString(registerEnBytes);
        string[] registerData = registerCodificado.Split("/");

        string mensaje = "";

        NuevorServidor.Clases.User newUser = null;

        if (_singleton.ValidateData(registerData[1]))
        {
            newUser = new NuevorServidor.Clases.User(registerData[0], registerData[1], registerData[2]);
            _singleton.AddUser(newUser);

            Console.WriteLine($"Usuario creado");
            Console.WriteLine($"nombre: {registerData[0]}");
            Console.WriteLine($"email: {registerData[1]}");
            Console.WriteLine($"contraseña: {registerData[2]}");
            mensaje = "Usuario registrado exitosamente";
        }
        else
        {
            mensaje = "Email ya registrado";
        }

        byte[] mensajeEnByte = Encoding.UTF8.GetBytes(mensaje);

        // enviar el header
        Header encabezadoEnvio = new Header(Common.Protocol.Request,
            Commands.Register,
            mensajeEnByte.Length);

        byte[] encabezadoEnvioEnBytes = encabezadoEnvio.GetBytesFromHeader();
        networkHelper.Send(encabezadoEnvioEnBytes);

        // enviar register info
        networkHelper.Send(mensajeEnByte);

        return newUser;
    }

    static async Task ListarUsuariosConBusquedaAsync(NetworkHelper networkHelper, Header encabezado, NetworkStream networkStream)
    {

        // recibe un mensaje

        byte[] mensajeBytes = await networkHelper.ReceiveAsync(encabezado.largoDeDatos);
        string mensajeCodificado = Encoding.UTF8.GetString(mensajeBytes);
        List<UserDetail> users = _singleton.UsersWithCoincidences(mensajeCodificado);
        string mensajeRetorno;
        if (users.Count == 0)
        {
            mensajeRetorno = "No hay usuarios que tengan coincidencias con esa palabra";

        }
        else
        {
            mensajeRetorno = "Los perfiles que tienen coincidencias con la búsqueda son :" + "\n";

            foreach (UserDetail userD in users)
            {
                mensajeRetorno += userD.UserEmail + "\n" + userD.Description + "\n" + userD.Skills + "\n" + "\n";

            }
        }
        byte[] mensajeEnByte = Encoding.UTF8.GetBytes(mensajeRetorno);

        Header encabezadoEnvio = new Header(Common.Protocol.Request,
            Commands.ListUsers,
            mensajeEnByte.Length);

        byte[] encabezadoEnvioEnBytes = encabezadoEnvio.GetBytesFromHeader();
        networkHelper.Send(encabezadoEnvioEnBytes);

        // enviar lista de usuarios
        networkHelper.Send(mensajeEnByte);

    }

    static async Task ListarUsuarioEspecificoAsync(NetworkHelper networkHelper, Header encabezado, NetworkStream networkStream)
    {

        // recibe un mensaje
        string avisoSiHayFoto = "No";
        byte[] mensajeBytes = await networkHelper.ReceiveAsync(encabezado.largoDeDatos);
        string mensajeCodificado = Encoding.UTF8.GetString(mensajeBytes);
        UserDetail userD = _singleton.SpecificUserProfile(mensajeCodificado);
        string mensajeRetorno = "";
        string fileName = "";
        if (userD == null)
        {
            mensajeRetorno = "No hay perfiles de usuarios con ese email";

        }
        else
        {
            fileName = userD.PhotoName;
            mensajeRetorno = userD.UserEmail + "\n" + userD.Description + "\n" + userD.Skills + "\n";
        }
        byte[] mensajeEnByte = Encoding.UTF8.GetBytes(mensajeRetorno);

        Header encabezadoEnvio = new Header(Common.Protocol.Request,
            Commands.ListUsers,
            mensajeEnByte.Length);

        byte[] encabezadoEnvioEnBytes = encabezadoEnvio.GetBytesFromHeader();
        //aviso si hay foto en el perfil de usuario
        //envio file a server
        if (!fileName.Equals(""))
        {
            avisoSiHayFoto = "Si";
            byte[] avisoFotoByte = Encoding.UTF8.GetBytes(avisoSiHayFoto);

            Header encabezadoAvisoFoto = new Header(Common.Protocol.Request,
                Commands.ListUsers,
                avisoFotoByte.Length);

            networkHelper.Send(encabezadoAvisoFoto.GetBytesFromHeader());
            networkHelper.Send(avisoFotoByte);
            var fileCommonHandler = new FileCommsHandler(networkHelper);
            fileCommonHandler.SendFile(Path.GetFullPath(fileName));
        }
        else
        {
            byte[] avisoFotoByte = Encoding.UTF8.GetBytes(avisoSiHayFoto);

            Header encabezadoAvisoFoto = new Header(Common.Protocol.Request,
                Commands.ListUsers,
                avisoFotoByte.Length);

            networkHelper.Send(encabezadoAvisoFoto.GetBytesFromHeader());
            networkHelper.Send(avisoFotoByte);
        }
        networkHelper.Send(encabezadoEnvioEnBytes);

        // enviar lista de usuarios
        networkHelper.Send(mensajeEnByte);

    }


    static async Task CrearPerfilLaboralAsync(NetworkHelper networkHelper, Header encabezado, NuevorServidor.Clases.User user, NetworkStream networkStream)
    {
        if (user == null)
        {
            throw new Exception("User not found");
        }

        // recibe un mensaje
        byte[] mensajeEnBytes = await networkHelper.ReceiveAsync(encabezado.largoDeDatos);
        string mensajeCodificado = Encoding.UTF8.GetString(mensajeEnBytes);
        string[] data = mensajeCodificado.Split("/");

        //logica
        UserDetail newDetails = new UserDetail(user.Email, data[1], data[0]);
        _singleton.AddDetail(newDetails);

        Console.WriteLine($"Perfil creado");
        Console.WriteLine($"habilidades: {data[0]}");
        Console.WriteLine($"descripcion: {data[1]}");
        string mensaje = "Detalles ingresados exitosamente";

        // enviar el header
        byte[] mensajeEnByte = Encoding.UTF8.GetBytes(mensaje);
        Header encabezadoEnvio = new Header(Common.Protocol.Request,
            Commands.JobProfile,
            mensajeEnByte.Length);

        byte[] encabezadoEnvioEnBytes = encabezadoEnvio.GetBytesFromHeader();
        networkHelper.Send(encabezadoEnvioEnBytes);

        networkHelper.Send(mensajeEnByte);
    }

    static async Task SubirFotoAsync(NetworkHelper networkHelper, Header encabezado, NuevorServidor.Clases.User user, NetworkStream networkStream)
    {
        string mensaje = "Es necesario tener un perfil laboral para asociarle una foto";
        if (user == null)
        {
            throw new Exception("User not found");
        }
        bool tienePerfil = _singleton.UserProfileExists(user);
        // recibe un mensaje

        byte[] FileExistsEnBytes = await networkHelper.ReceiveAsync(encabezado.largoDeDatos);
        string fileExistsCodificado = Encoding.UTF8.GetString(FileExistsEnBytes);
        if (fileExistsCodificado.Equals("Si"))
        {
            var fileCommonHandler = new FileCommsHandler(networkHelper);
            var fileName = await fileCommonHandler.ReceiveFileAsync();
            if (tienePerfil)
            {
                _singleton.SetUserFotoName(user, fileName);

                //logica
                Console.WriteLine($"Foto subida");

                mensaje = "Foto subida exitosamente";
            }
        }
        else
        {
            mensaje = "Esa imagen no existe";
        }
        // enviar el header
        byte[] mensajeEnByte = Encoding.UTF8.GetBytes(mensaje);
        Header encabezadoEnvio = new Header(Common.Protocol.Request,
            Commands.ProfilePic,
            mensajeEnByte.Length);

        byte[] encabezadoEnvioEnBytes = encabezadoEnvio.GetBytesFromHeader();
        networkHelper.Send(encabezadoEnvioEnBytes);

        networkHelper.Send(mensajeEnByte);

    }

    static async Task LeerChatAsync(NetworkHelper networkHelper, Header encabezado, NuevorServidor.Clases.User loggedUser, NetworkStream networkStream)
    {

        byte[] chatEnBytes = await networkHelper.ReceiveAsync(encabezado.largoDeDatos);
        string chatCodificado = Encoding.UTF8.GetString(chatEnBytes);
        string mensaje = _singleton.LeerChat(loggedUser.Email, chatCodificado);

        if (mensaje.Equals(""))
        {
            mensaje = "No se han recibido mensajes de este usuario";
        }

        //envio
        byte[] mensajeLogInEnByte = Encoding.UTF8.GetBytes(mensaje);

        Header encabezadoLogInEnvio = new Header(Common.Protocol.Request,
            Commands.ReadChat,
            mensajeLogInEnByte.Length);


        byte[] encabezadoLogInEnvioEnBytes = encabezadoLogInEnvio.GetBytesFromHeader();
        networkHelper.Send(encabezadoLogInEnvioEnBytes);

        networkHelper.Send(mensajeLogInEnByte);

    }
    static async Task EnviarChatAsync(NetworkHelper networkHelper, Header encabezado, NuevorServidor.Clases.User loggedUser, NetworkStream networkStream)
    {

        byte[] chatEnBytes = await networkHelper.ReceiveAsync(encabezado.largoDeDatos);
        string chatCodificado = Encoding.UTF8.GetString(chatEnBytes);
        string[] chatData = chatCodificado.Split("/");
        _singleton.EnviarChat(loggedUser.Email, chatData[0], chatData[1]);

    }


    static async Task HandleClient(TcpListener tcpListener)
    {
        var tcpClientSocket = await tcpListener.AcceptTcpClientAsync().ConfigureAwait(false);
        Console.WriteLine("Un nuevo cliente establecio conexión");
        clients.Add(tcpClientSocket);
        // Acepte un cliente y estoy conectado 
        bool conectado = true;
        NuevorServidor.Clases.User user = null;
        using (var networkStream = tcpClientSocket.GetStream())
        {
            while (conectado)
            {


                NetworkHelper networkHelper = new NetworkHelper(networkStream);
                Header encabezado = new Header();

                byte[] encabezadoEnBytes = await networkHelper.ReceiveAsync(Common.Protocol.Request.Length + Common.Protocol.CommandLength + Common.Protocol.DataLengthLength);
                encabezado.DecodeHeader(encabezadoEnBytes);
                try
                {
                    switch (encabezado.comando)
                    {
                        case Commands.Register:
                            user = await RegisterAsync(networkHelper, encabezado, networkStream);
                            break;

                        case Commands.Login:
                            user = await LoginAsync(networkHelper, encabezado, networkStream);
                            break;

                        case Commands.JobProfile:
                            await CrearPerfilLaboralAsync(networkHelper, encabezado, user, networkStream);
                            break;
                        case Commands.ProfilePic:
                            await SubirFotoAsync(networkHelper, encabezado, user, networkStream);
                            break;

                        case Commands.ListUsers:
                            await ListarUsuariosConBusquedaAsync(networkHelper, encabezado, networkStream);
                            break;

                        case Commands.ReadChat:
                            await LeerChatAsync(networkHelper, encabezado, user, networkStream);
                            break;

                        case Commands.SendChat:
                            await EnviarChatAsync(networkHelper, encabezado, user, networkStream);
                            break;

                        case Commands.ListSpecificUser:
                            await ListarUsuarioEspecificoAsync(networkHelper, encabezado, networkStream);
                            break;
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine("Se desconecto el cliente");
                    conectado = false;
                }
            }
            Console.WriteLine("Cerrando conexión con cliente...");
            networkStream.Close();
            tcpClientSocket.Close();

        }
        Console.WriteLine("Cerrando conexión con cliente...");
    }
}
