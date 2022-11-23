using Microsoft.AspNetCore.Mvc;
using Grpc.Net.Client;
using NuevorServidor.Models;

namespace ServerAdmin.Controllers;

[ApiController]
[Route("[controller]")]
public class PhotoController : ControllerBase
{
    private readonly ILogger<PhotoController> _logger;

    public PhotoController(ILogger<PhotoController> logger)
    {
        _logger = logger;
    }


    [HttpDelete]
    public string EliminarFoto([FromBody] UserEmailModelo userEmail)
    {
        using var channel = GrpcChannel.ForAddress("http://localhost:5024");
        var client = new Photo.PhotoClient(channel);
        var reply = client.EliminarFoto(new PhotoPerfilIdentifier
        {
            Email = userEmail.email,
        });
        return reply.Message;
    }
}
