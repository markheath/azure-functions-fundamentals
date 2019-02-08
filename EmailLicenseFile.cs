using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;

namespace pluralsightfuncs
{
    public static class EmailLicenseFile
    {
        [FunctionName("EmailLicenseFile")]
        public static void Run([BlobTrigger("licenses/{orderId}.lic", 
            Connection = "AzureWebJobsStorage")]string licenseFileContents,
            [SendGrid(ApiKey = "SendGridApiKey")] ICollector<SendGridMessage> sender,
            [Table("orders", "orders", "{orderId}")] Order order,
            string orderId, 
            ILogger log)
        {
            var email = order.Email;
            log.LogInformation($"Got order from {email}\n Order Id:{orderId}");
            var message = new SendGridMessage();
            message.From = new EmailAddress(Environment.GetEnvironmentVariable("EmailSender"));
            message.AddTo(email);
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(licenseFileContents);
            var base64 = Convert.ToBase64String(plainTextBytes);
            message.AddAttachment($"{orderId}.lic", base64, "text/plain");
            message.Subject = "Your license file";
            message.HtmlContent = "Thank you for your order";
            if (!email.EndsWith("@test.com"))
                sender.Add(message);
        }
    }
}
