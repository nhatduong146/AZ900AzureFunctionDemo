using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AZ900AzureFunctionDemo
{
    public class HttpTriggerFunction
    {
        private readonly ILogger _logger;

        public HttpTriggerFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HttpTriggerFunction>();
        }

        [Function("HttpTriggerFunction")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string messageContent = req.Query["message"] ?? "Default message";

            string serviceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
            string queueName = Environment.GetEnvironmentVariable("QueueName");

            await SendMessageToServiceBusQueue(serviceBusConnectionString, queueName, messageContent);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Your message has been sent to service bus queue successfully");

            return response;
        }

        public async Task SendMessageToServiceBusQueue(string connectionString, string queueName, string messageContent)
        {
            await using (ServiceBusClient client = new ServiceBusClient(connectionString))
            {
                ServiceBusSender sender = client.CreateSender(queueName);
                ServiceBusMessage message = new ServiceBusMessage(messageContent);
                await sender.SendMessageAsync(message);
            }
        }
    }
}
