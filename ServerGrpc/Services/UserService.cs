using ServerGrpc;
using ServerGrpc.Clases;
using Grpc.Core;
using ServerGrpc;
using Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Communication;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace ServerGrpc.Services;

public class UserService : ServerGrpc.User.UserBase
{


    public override Task<MessageReply> PostUser(UserDTO userToAdd, ServerCallContext context)
    {
        string message = "";
        if (Server._singleton.ValidateData(userToAdd.Email))
        {
            Server._singleton.AddUser(new ServerGrpc.Clases.User(userToAdd.Name, userToAdd.Email, userToAdd.Password));
            message = "Usuario creado correctamente";
        }
        else
        {
            message = "No se pudo crear usuario";
        }
        return Task.FromResult(new MessageReply { Message = message });
    }

    public override Task<MessageReply> DeleteUser(ServerGrpc.Id userToDelete, ServerCallContext context)
    {
        bool couldPost = Server._singleton.DeleteUser(userToDelete.Email);
        string message = couldPost ? "Usuario eliminado correctamente" : "No se pudo eliminar usuario";
        return Task.FromResult(new MessageReply { Message = message });
    }

    public override Task<MessageReply> EditUser(UserDTO userToEdit, ServerCallContext context)
    {
        bool couldPost = Server._singleton.EditUser(userToEdit.Name, userToEdit.Email, userToEdit.Password);
        string message = couldPost ? "Usuario editado correctamente" : "No se pudo editar usuario";
        return Task.FromResult(new MessageReply { Message = message });
    }
}