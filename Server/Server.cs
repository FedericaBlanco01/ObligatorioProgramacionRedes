using Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Server.Clases;
using Communication;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

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

        Common.SettingsManager.SetupConfiguration(ConfigurationManager.AppSettings);

        var localEndpoint = new IPEndPoint(IPAddress.Parse(Common.SettingsManager.IpServer), Int32.Parse(Common.SettingsManager.PortServer));

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

    static User Login(NetworkHelper networkHelper, Header encabezado, Singleton system)
    {
        byte[] loginEnBytes = networkHelper.Receive(encabezado.largoDeDatos);
        string loginCodificado = Encoding.UTF8.GetString(loginEnBytes);
        string[] loginData = loginCodificado.Split("/");

        User loggedUser = system.LoginBack(loginData[0], loginData[1]);
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

    static void ListarUsuariosConBusqueda(NetworkHelper networkHelper, Header encabezado, Singleton system) {

        // recibe un mensaje

        byte[] mensajeBytes = networkHelper.Receive(encabezado.largoDeDatos);
        string mensajeCodificado = Encoding.UTF8.GetString(mensajeBytes);
        List<UserDetail> users = system.UsersWithCoincidences(mensajeCodificado);
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

    static void ListarUsuarioEspecifico(NetworkHelper networkHelper, Header encabezado, Singleton system, Socket cliente)
    {

        // recibe un mensaje
        string avisoSiHayFoto = "No";
        byte[] mensajeBytes = networkHelper.Receive(encabezado.largoDeDatos);
        string mensajeCodificado = Encoding.UTF8.GetString(mensajeBytes);
        UserDetail userD = system.SpecificUserProfile(mensajeCodificado);
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
            var fileCommonHandler = new FileCommsHandler(cliente);
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


    static void CrearPerfilLaboral(NetworkHelper networkHelper, Header encabezado, Singleton system, User user)
    {
        if (user == null)
        {
            throw new Exception("User not found");
        }

        // recibe un mensaje
        byte[] mensajeEnBytes = networkHelper.Receive(encabezado.largoDeDatos);
        string mensajeCodificado = Encoding.UTF8.GetString(mensajeEnBytes);
        string[] data = mensajeCodificado.Split("/");

        //logica
        UserDetail newDetails = new UserDetail(user.Email, data[1], data[0]);
        system.AddDetail(newDetails);

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

    static void SubirFoto(NetworkHelper networkHelper, Header encabezado, Singleton system, User user, Socket cliente)
    {
        string mensaje = "Es necesario tener un perfil de usuario para asociarle una foto";
        if (user == null)
        {
            throw new Exception("User not found");
        }
        bool tienePerfil = system.UserProfileExists(user);
        // recibe un mensaje
        
        byte[] FileExistsEnBytes = networkHelper.Receive(encabezado.largoDeDatos);
        string fileExistsCodificado = Encoding.UTF8.GetString(FileExistsEnBytes);

        if (fileExistsCodificado.Equals("Si"))
        {
            var fileCommonHandler = new FileCommsHandler(cliente);
            var fileName = fileCommonHandler.ReceiveFile();
            Console.WriteLine("llegue");
            if (tienePerfil)
            {
                system.SetUserFotoName(user, fileName);

                //logica
                Console.WriteLine($"Foto subida");

                mensaje = "Foto subida exitosamente";
            }
        }
        else {
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

    static void LeerChat(NetworkHelper networkHelper, Header encabezado, Singleton system, User loggedUser) {

        byte[] chatEnBytes = networkHelper.Receive(encabezado.largoDeDatos);
        string chatCodificado = Encoding.UTF8.GetString(chatEnBytes);
        string mensaje = system.LeerChat(loggedUser.Email,chatCodificado);
        
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
    static void EnviarChat(NetworkHelper networkHelper, Header encabezado, Singleton system, User loggedUser)
    {

        byte[] chatEnBytes = networkHelper.Receive(encabezado.largoDeDatos);
        string chatCodificado = Encoding.UTF8.GetString(chatEnBytes);
        string[] chatData = chatCodificado.Split("/");
        system.EnviarChat(loggedUser.Email, chatData[0], chatData[1]);

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
                    networkHelper.Receive(Common.Protocol.Request.Length + Common.Protocol.CommandLength + Common.Protocol.DataLengthLength);
                encabezado.DecodeHeader(encabezadoEnBytes);

                switch (encabezado.comando)
                {
                    case Commands.Register:
                        user = Register(networkHelper, encabezado, system);
                        break;

                    case Commands.Login:
                        user = Login(networkHelper, encabezado, system);
                        break;

                    case Commands.JobProfile:
                        CrearPerfilLaboral(networkHelper, encabezado, system, user);
                        break;
                    case Commands.ProfilePic:
                        SubirFoto(networkHelper, encabezado, system, user, cliente);
                        break;

                    case Commands.ListUsers:
                        ListarUsuariosConBusqueda(networkHelper, encabezado, system);
                        break;

                    case Commands.ReadChat:
                        LeerChat(networkHelper, encabezado, system, user);
                        break;

                    case Commands.SendChat:
                        EnviarChat(networkHelper, encabezado, system, user);
                        break;

                    case Commands.ListSpecificUser:
                        ListarUsuarioEspecifico(networkHelper, encabezado, system, cliente);
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


