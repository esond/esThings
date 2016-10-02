using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using esThings.Devices;

namespace esThings.Controllers
{
    public class AnalyzeController : Controller
    {
        // GET: Analyze
        public ActionResult Index()
        {
            List<GarbageCanStatus> statuses = new List<GarbageCanStatus>();

            statuses.Add(new GarbageCanStatus {MessageId = Guid.NewGuid(), DeviceId = "can1", DeviceKey = "theCanNumber1", Fullness = 50});
            statuses.Add(new GarbageCanStatus {MessageId = Guid.NewGuid(), DeviceId = "can2", DeviceKey = "theCanNumber2", Fullness = 100});

            return View(statuses);
        }
    }
}