using NuevorServidor;
using NuevorServidor.Clases;

public class UserService 
{

    public static Singleton session = new Singleton();

    static Task<MessageReply> PostUser(UserDTO userToAdd)
    {
        string message = "";
        if (session.ValidateData(userToAdd.Email))
        {
            session.AddUser(new NuevorServidor.Clases.User(userToAdd.Name, userToAdd.Email, userToAdd.Password ));
            message = "Usuario creado correctamente";
        }
        else
        {
            message = "No se pudo crear usuario";
        }
        return Task.FromResult(new MessageReply { Message = message });
    }

    /*
    static async Task<MessageReply> DeleteUser(Id userToDelete)
    {
        bool couldPost = session.DeleteUser(userToDelete.id);
        string message = couldPost ? "Usuario creado correctamente" : "No se pudo crear usuario";
        return Task.FromResult(new MessageReply { Message = message });
    }*/
}