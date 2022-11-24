using Microsoft.AspNetCore.Mvc;
using Grpc.Net.Client;
using ServerGrpc.Models;
using Common;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace ServerAdmin.Controllers;

[ApiController]
[Route("[controller]")]
public class PhotoController : ControllerBase
{
    private readonly ILogger<PhotoController> _logger;

    
    
    public PhotoController(ILogger<PhotoController> logger)
    {
        SettingsManager.SetupGrpcConfiguration(ConfigurationManager.AppSettings);
        _logger = logger;
    }


    [HttpDelete]
    public async Task<string> EliminarFoto([FromBody] UserEmailModelo userEmail)
    {
        
        using var channel = GrpcChannel.ForAddress(SettingsManager.GrpcAddress);
        var client = new Photo.PhotoClient(channel);
        var reply = await client.EliminarFotoAsync(new PhotoPerfilIdentifier
        {
            Email = userEmail.email,
        });
        return reply.Message;
    }
}
