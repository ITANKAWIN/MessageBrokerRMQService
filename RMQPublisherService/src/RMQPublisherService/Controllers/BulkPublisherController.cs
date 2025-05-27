using Microsoft.AspNetCore.Mvc;
using RMQPublisherService.Models;
using RMQPublisherService.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RMQPublisherService.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BulkPublisherController : ControllerBase
    {
        private readonly IPublisherService _service;

        public BulkPublisherController(IPublisherService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IEnumerable<ResponseModel>> SendSMSBulk([FromBody] BulkSmsRequestModel request)
        {
            var tasks = new List<Task<ResponseModel>>();
            for (int i = 0; i < request.Count; i++)
            {
                var model = new ReqestSMSEventModel
                {
                    message = request.Message,
                    mobileNumber = request.MobileNumber,
                    systemName = request.SystemName
                };
                tasks.Add(_service.SendSMS(model));
            }
            var results = await Task.WhenAll(tasks);
            return results;
        }

        [HttpPost]
        public async Task<IEnumerable<ResponseModel>> SendNotiBulk([FromBody] BulkNotiRequestModel request)
        {
            var tasks = new List<Task<ResponseModel>>();
            for (int i = 0; i < request.Count; i++)
            {
                var model = new ReqestNotiEventModel
                {
                    message = request.Message,
                    deviceId = request.DeviceId,
                    systemName = request.SystemName
                };
                tasks.Add(_service.SendNoti(model));
            }
            var results = await Task.WhenAll(tasks);
            return results;
        }
    }
}
