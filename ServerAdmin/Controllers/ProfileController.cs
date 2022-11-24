using Microsoft.AspNetCore.Mvc;
using Grpc.Net.Client;
using NuevorServidor.Models;
using Common;
using ConfigurationManager = System.Configuration.ConfigurationManager;
namespace ServerAdmin.Controllers;

[ApiController]
[Route("[controller]")]
public class ProfileController : ControllerBase
{
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(ILogger<ProfileController> logger)
    {
        SettingsManager.SetupGrpcConfiguration(ConfigurationManager.AppSettings);
        _logger = logger;
    }

    [HttpPost]
    public async Task<string> CrearPerfil([FromBody] PerfilModelo perfil)
    {
        using var channel = GrpcChannel.ForAddress(SettingsManager.GrpcAddress);
        var client = new Perfil.PerfilClient(channel);
        var reply = await client.CrearPerfilAsync(new PerfilData
        {
            Email = perfil.email,
            Descripcion = perfil.descripcion,
            Habilidades = perfil.habilidades,
        });
        return reply.Message;
    }

    [HttpDelete]
    public async Task<string> EliminarPerfil([FromBody] UserEmailModelo userEmail)
    {
        using var channel = GrpcChannel.ForAddress(SettingsManager.GrpcAddress);
        var client = new Perfil.PerfilClient(channel);
        var reply = await client.EliminarPerfilAsync(new PerfilIdentifier
        {
            Email = userEmail.email,
        });
        return reply.Message;
    }

    [HttpPut]
    public async Task<string> EditarPerfil([FromBody] PerfilModelo perfil)
    {
        using var channel = GrpcChannel.ForAddress(SettingsManager.GrpcAddress);
        var client = new Perfil.PerfilClient(channel);
        var reply = await client.EditarPerfilAsync(new PerfilData
        {
            Email = perfil.email,
            Descripcion = perfil.descripcion,
            Habilidades = perfil.habilidades,
        });
        return reply.Message;
    }
}
