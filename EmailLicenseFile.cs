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
        public static void Run([BlobTrigger("licenses/{name}", Connection = "AzureWebJobsStorage")]string licenseFileContents, 
            [SendGrid(ApiKey = "SendGridApiKey")] out SendGridMessage message,
            string name, ILogger log)
        {
            var email = Regex.Match(licenseFileContents, 
                @"^Email\:\ (.+)$", RegexOptions.Multiline).Groups[1].Value;
            log.LogInformation($"Got order from {email}\n License file Name:{name}");
            message = new SendGridMessage();
            message.From = new EmailAddress(Environment.GetEnvironmentVariable("EmailSender"));
            message.AddTo(email);
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(licenseFileContents);
            var base64 = Convert.ToBase64String(plainTextBytes);
            message.AddAttachment(name, base64, "text/plain");
            message.Subject = "Your license file";
            message.HtmlContent = "Thank you for your order";
        }
    }
}
