using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
namespace ServerAdmin.Controllers
{
    [Route("User")]
    [ApiController]
    public class UserController : Controller
    {
        private User.UserClient client;
        private string grpcURL = "http://localhost:5024";
        private readonly ILogger<UserController> _logger;
        public UserController(ILogger<UserController> logger)
        {
            AppContext.SetSwitch(
                  "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
           _logger = logger;
        }

        [HttpPost]
        public async Task<string> PostUser([FromBody] UserDTO user)
        {
            using var channel = GrpcChannel.ForAddress(grpcURL);
            client = new User.UserClient(channel);
            var reply = await client.PostUserAsync(user);
            return (reply.Message);
        }
/*
        [HttpPut]
        public async Task<IActionResult> EditUserAsync([FromBody] UserDTO user)
        {
            using var channel = GrpcChannel.ForAddress(grpcURL);
            client = new(channel);
            var reply = await client.EditUserAsync(user);
            return Ok(reply.Message);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUserAsync([FromQuery] Id id)
        {
            using var channel = GrpcChannel.ForAddress(grpcURL);
            client = new(channel);
            var reply = await client.DeleteUserAsync(id);
            return Ok(reply.Message);
        }*/
    }
}
