using Microsoft.AspNetCore.Mvc;
using Grpc.Net.Client;
using NuevorServidor.Models;

namespace ServerAdmin.Controllers;

[ApiController]
[Route("[controller]")]
public class ProfileController : ControllerBase
{
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(ILogger<ProfileController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public string CrearPerfil([FromBody] PerfilModelo perfil)
    {
        using var channel = GrpcChannel.ForAddress("http://localhost:5024");
        var client = new Perfil.PerfilClient(channel);
        var reply = client.CrearPerfil(new PerfilData
        {
            Email = perfil.email,
            Descripcion = perfil.descripcion,
            Habilidades = perfil.habilidades,
        });
        return reply.Message;
    }

    [HttpDelete]
    public string EliminarPerfil([FromBody] UserEmailModelo userEmail)
    {
        using var channel = GrpcChannel.ForAddress("http://localhost:5024");
        var client = new Perfil.PerfilClient(channel);
        var reply = client.EliminarPerfil(new PerfilIdentifier
        {
            Email = userEmail.email,
        });
        return reply.Message;
    }

    [HttpPut]
    public string EditarPerfil([FromBody] PerfilModelo perfil)
    {
        using var channel = GrpcChannel.ForAddress("http://localhost:5024");
        var client = new Perfil.PerfilClient(channel);
        var reply = client.EditarPerfil(new PerfilData
        {
            Email = perfil.email,
            Descripcion = perfil.descripcion,
            Habilidades = perfil.habilidades,
        });
        return reply.Message;
    }
}
