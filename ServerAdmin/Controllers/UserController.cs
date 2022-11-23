using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using NuevorServidor.Models;
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
        public async Task<string> PostUser([FromBody] UserModel user)
        {
            using var channel = GrpcChannel.ForAddress(grpcURL);
            client = new User.UserClient(channel);
            var reply = await client.PostUserAsync(new UserDTO
            {
                Email = user.Email,
                Name = user.Name,
                Password = user.Password
            });
            return (reply.Message);
        }

        [HttpPut]
        public async Task<string> EditUserAsync([FromBody] UserDTO user)
        {
            using var channel = GrpcChannel.ForAddress(grpcURL);
            client = new(channel);
            var reply = await client.EditUserAsync(user);
            return (reply.Message);
        }

        [HttpDelete]
        public async Task<string> DeleteUserAsync([FromBody] Id id)
        {
            using var channel = GrpcChannel.ForAddress(grpcURL);
            client = new(channel);
            var reply = await client.DeleteUserAsync(id);
            return (reply.Message);
        }
    }
}
