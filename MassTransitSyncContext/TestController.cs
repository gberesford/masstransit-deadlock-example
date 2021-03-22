using MassTransit;
using Serilog;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace MassTransitSyncContext
{
    [RoutePrefix("api/test")]
    public class TestController : ApiController
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger _logger;

        public TestController(IPublishEndpoint publishEndpoint, ILogger logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        // This method can be called as <base url>/api/test/async, and it works fine
        [HttpGet]
        [Route("async")]
        public async Task<HttpResponseMessage> Async()
        {
            await PublishMessage();
            return new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("It worked!")
            };
        }

        // This method can be called as <base url>/api/test/sync, and *if the async version hasn't previously been called* it deadlocks
        [HttpGet]
        [Route("sync")]
        public HttpResponseMessage Sync()
        {
            PublishMessage().Wait();
            return new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("This will never be executed")
            };
        }

        private async Task PublishMessage()
        {
            _logger.Information("Publish started");
            await _publishEndpoint.Publish<TestMessage>(new { Timestamp = DateTime.UtcNow.ToString() }).ConfigureAwait(continueOnCapturedContext: false);
            _logger.Information("Publish completed");
        }
    }
}
