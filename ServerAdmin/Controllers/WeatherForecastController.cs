using Microsoft.AspNetCore.Mvc;
using Grpc.Net.Client;

namespace ServerAdmin.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public string Get()
    {
        using var channel = GrpcChannel.ForAddress("http://localhost:5024");
        var client = new Greeter.GreeterClient(channel);
        var reply = client.SayHello(new HelloRequest { Name = "GreeterClient" });
        return reply.Message;
    }
}
