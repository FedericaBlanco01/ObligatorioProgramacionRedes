using NuevorServidor;
using NuevorServidor.Clases;
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

public class UserService : NuevorServidor.User.UserBase
{


    public override Task<MessageReply> PostUser(UserDTO userToAdd, ServerCallContext context)
    {
        string message = "";
        if (GreeterService._singleton.ValidateData(userToAdd.Email))
        {
            GreeterService._singleton.AddUser(new NuevorServidor.Clases.User(userToAdd.Name, userToAdd.Email, userToAdd.Password));
            message = "Usuario creado correctamente";
        }
        else
        {
            message = "No se pudo crear usuario";
        }
        return Task.FromResult(new MessageReply { Message = message });
    }

    // static async Task<MessageReply> DeleteUser(Id userToDelete)
    // {
    //     bool couldPost = session.DeleteUser(userToDelete.id);
    //     string message = couldPost ? "Usuario creado correctamente" : "No se pudo crear usuario";
    //     return Task.FromResult(new MessageReply { Message = message });
    // }
}