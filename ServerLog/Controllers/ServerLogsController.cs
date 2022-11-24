using Microsoft.AspNetCore.Mvc;
using ServerLog.Data;
using ServerLog.Model;

namespace ServerLog.Controllers
{

    [ApiController]
    [Route("serverLogs")]
    public class ServerLogsController : ControllerBase
    {
        [HttpGet]
        public List<LogModel> FilterLogs([FromQuery] string? email, [FromQuery] string? date, [FromQuery] string? eventDone)
        {
            return DataAccess.GetInstance().filterLogs(email, date, eventDone);
        }

    }
}