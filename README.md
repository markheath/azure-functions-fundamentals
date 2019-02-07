# To test locally

Set up a `local.settings.json` file:

```
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "EmailSender": "your@email.address",
    "SendGridApiKey": "your SendGrid API key"
  }
}
```

You can then submit orders via powershell:

```
iwr -Method POST `
    -Uri http://localhost:7071/api/OnPaymentReceived `
    -Body '{ "OrderId":"123", "Email":"pluralsight@mailinator.com", "ProductId":"X101" }' `
    -Headers @{ "Content-Type"="application/json" }
```