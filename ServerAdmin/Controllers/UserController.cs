using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using Common;
namespace ServerAdmin.Controllers
{
    public class UserController : Controller
    {
        private SettingsManager settingsManager = new SettingsManager();
        private User.UserClient client;
        private string grpcURL;

        public UserController()
        {
            AppContext.SetSwitch(
                  "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            grpcURL = settingsManager.ReadSetting(ServerConfig.GrpcURL);
        }

        [HttpPost]
        public async Task<IActionResult> PostUserAsync([FromBody] UserDTO user)
        {
            using var channel = GrpcChannel.ForAddress(grpcURL);
            client = new (channel);
            var reply = await client.PostUserAsync(user);
            return Ok(reply.Message);
        }

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
        }
    }
}
