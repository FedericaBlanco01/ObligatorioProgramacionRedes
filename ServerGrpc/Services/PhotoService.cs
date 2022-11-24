using Grpc.Core;
using ServerGrpc;
using Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ServerGrpc.Clases;
using Communication;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace ServerGrpc.Services;

public class PhotoService : Photo.PhotoBase
{
    private readonly ILogger<PhotoService> _logger;
    public PhotoService(ILogger<PhotoService> logger)
    {
        _logger = logger;
    }

    public override Task<PhotoResponse> EliminarFoto(PhotoPerfilIdentifier request, ServerCallContext context)
    {
        Boolean could = Server._singleton.DeletePhoto(request.Email);
        string message = could ? "foto eliminada correctamente" : "No se pudo eliminar foto";
        return Task.FromResult(new PhotoResponse { Message = message });
    }
}