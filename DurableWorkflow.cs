using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace pluralsightfuncs
{
    public static class DurableWorkflow
    {
        [FunctionName(nameof(OnPaymentReceived2))]
        public static async Task<IActionResult> OnPaymentReceived2(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
            [OrchestrationClient] DurableOrchestrationClient durableClient,
            ILogger log)
        {
            log.LogInformation("Received a payment.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var order = JsonConvert.DeserializeObject<Order>(requestBody);
            log.LogInformation($"Order {order.OrderId} received from {order.Email}" +
                               $" for product {order.ProductId}");

            var orchestrationId = await durableClient.StartNewAsync(
                nameof(NewOrderWorkflow), order);
            var response = durableClient.CreateHttpManagementPayload(orchestrationId);
            return new OkObjectResult(response);
        }


        [FunctionName(nameof(NewOrderWorkflow))]
        public static async Task NewOrderWorkflow(
            [OrchestrationTrigger]DurableOrchestrationContext ctx,
            ILogger log)
        {
            var order = ctx.GetInput<Order>();
            if (!ctx.IsReplaying)
                log.LogInformation($"Starting new order workflow for {order.OrderId}.");
            await ctx.CallActivityAsync(nameof(SaveOrderToDatabaseActivity), order);
            await ctx.CallActivityAsync(nameof(CreateLicenseFileActivity), order);
            await ctx.CallActivityAsync(nameof(EmailLicenseFileActivity), order);
            log.LogInformation($"Order {order.OrderId} processed successfully.");

        }
        [FunctionName(nameof(SaveOrderToDatabaseActivity))]
        public static async Task SaveOrderToDatabaseActivity(
            [ActivityTrigger] Order order,
            [Table("orders")] IAsyncCollector<Order> orderTable,
            ILogger log)
        {
            order.PartitionKey = "orders"; // just one partition (for demo purposes)
            order.RowKey = order.OrderId;
            await orderTable.AddAsync(order);
        }

        [FunctionName(nameof(CreateLicenseFileActivity))]
        public static async Task CreateLicenseFileActivity(
            [ActivityTrigger] Order order,
            IBinder binder,
            ILogger log)
        {
            var outputBlob = await binder.BindAsync<TextWriter>(
                new BlobAttribute($"licenses/{order.OrderId}.lic"));

            outputBlob.WriteLine($"OrderId: {order.OrderId}");
            outputBlob.WriteLine($"Email: {order.Email}");
            outputBlob.WriteLine($"ProductId: {order.ProductId}");
            outputBlob.WriteLine($"PurchaseDate: {DateTime.UtcNow}");
            var md5 = System.Security.Cryptography.MD5.Create();
            var hash = md5.ComputeHash(
                Encoding.UTF8.GetBytes(order.Email + "secret"));
            outputBlob.WriteLine($"SecretCode: {BitConverter.ToString(hash).Replace("-", "")}");
        }

        [FunctionName(nameof(EmailLicenseFileActivity))]
        public static async Task EmailLicenseFileActivity(
            [ActivityTrigger] Order order,
            [SendGrid(ApiKey = "SendGridApiKey")] ICollector<SendGridMessage> sender,
            IBinder binder,
            ILogger log)
        {
            var licenseFileContents = await binder.BindAsync<string>(
                new BlobAttribute($"licenses/{order.OrderId}.lic"));

            var email = order.Email;
            log.LogInformation($"Got order from {email}\n Order Id:{order.OrderId}");
            var message = new SendGridMessage();
            message.From = new EmailAddress(Environment.GetEnvironmentVariable("EmailSender"));
            message.AddTo(email);
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(licenseFileContents);
            var base64 = Convert.ToBase64String(plainTextBytes);
            message.AddAttachment($"{order.OrderId}.lic", base64, "text/plain");
            message.Subject = "Your license file";
            message.HtmlContent = "Thank you for your order";
            if (!email.EndsWith("@test.com"))
                sender.Add(message);
        }
    }
}
