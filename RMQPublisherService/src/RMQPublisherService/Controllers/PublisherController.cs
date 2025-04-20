using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Models;
using RMQPublisherService.Models;
using RMQPublisherService.Services.Interfaces;

namespace RMQPublisherService.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PublisherController : ControllerBase
    {
        private readonly IPublisherService service;

        public PublisherController(IPublisherService service) 
        {
            this.service = service;
        }

        [HttpPost]
        public async Task<ResponseModel> SendSMS([FromBody] ReqestSMSEventModel request)
        {
            return await service.SendSMS(request);
        }

        [HttpPost]
        public async Task<ResponseModel> SendNoti([FromBody] ReqestNotiEventModel request)
        {
            return await service.SendNoti(request);
        }
    }
}