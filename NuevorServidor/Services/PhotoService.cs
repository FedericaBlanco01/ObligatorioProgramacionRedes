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

public class PhotoService : Photo.PhotoBase
{
    private readonly ILogger<PhotoService> _logger;
    public PhotoService(ILogger<PhotoService> logger)
    {
        _logger = logger;
    }

    public override Task<PhotoResponse> EliminarFoto(PhotoPerfilIdentifier request, ServerCallContext context)
    {
        GreeterService._singleton.DeletePhoto(request.Email);
        return Task.FromResult(new PhotoResponse { Message = "Foto eliminada" });
    }
}