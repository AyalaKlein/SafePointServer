using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SafePoint.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlertController : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult> GenerateAnAlert(double locX, double locY, double meterRadius)
        {
            var message = new Message()
            {
                Data = new Dictionary<string, string>
                {
                    ["locX"] = locX.ToString(),
                    ["locY"] = locY.ToString(),
                    ["meterRadius"] = meterRadius.ToString()
                },
                Topic = "israel-alerts"
            };
            await FirebaseMessaging.DefaultInstance.SendAsync(message);
            return Ok();
        }
    }
}
 