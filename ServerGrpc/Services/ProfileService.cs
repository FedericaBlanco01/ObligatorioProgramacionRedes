using Grpc.Core;
using ServerGrpc;
using Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ServerGrpc.Clases;


namespace ServerGrpc.Services;

public class ProfileService : Perfil.PerfilBase
{
    private readonly ILogger<ProfileService> _logger;
    public ProfileService(ILogger<ProfileService> logger)
    {
        _logger = logger;
    }

    public override Task<Response> CrearPerfil(PerfilData request, ServerCallContext context)
    {
        UserDetail newDetails = new UserDetail(request.Email, request.Descripcion, request.Habilidades);
        if (Server._singleton.UserExists(request.Email))
        {
            Server._singleton.AddDetail(newDetails);
            return Task.FromResult(new Response { Message = "Perfil creado" });
        }
        else
        {
            return Task.FromResult(new Response { Message = "Email no coincide con ningun usuario" });
        }
    }

    public override Task<Response> EliminarPerfil(PerfilIdentifier request, ServerCallContext context)
    {
        if (Server._singleton.DeleteDetail(request.Email))
        {
            return Task.FromResult(new Response { Message = "Perfil eliminado" });
        }
        else
        {
            return Task.FromResult(new Response { Message = "Perfil no encontrado" });
        }
    }

    public override Task<Response> EditarPerfil(PerfilData request, ServerCallContext context)
    {
        if (Server._singleton.EditDetail(request.Email, request.Descripcion, request.Habilidades))
        {

            return Task.FromResult(new Response { Message = "Perfil editado" });
        }
        else
        {
            return Task.FromResult(new Response { Message = "Perfil no encontrado" });
        }
    }

}
