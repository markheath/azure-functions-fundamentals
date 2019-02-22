# example 1 - calling with a valid function key (n.b. this key is no longer valid!)
iwr -Method POST `
    -Uri "https://pluralsightfuncsarm.azurewebsites.net/api/OnPaymentReceived?code=NEq9oTOPsjdRQz4/Y63X9128BOoHsozr8/J3uws59X3neC40e7RVPg==" `
    -Body '{ "OrderId":"200", "Email":"example@test.com", "ProductId":"X200" }' `
    -Headers @{ "Content-Type"="application/json" }
    
# example 2 - calling without a function key (will get 401 unauthorized)
iwr -Method POST `
    -Uri "https://pluralsightfuncsarm.azurewebsites.net/api/OnPaymentReceived" `
    -Body '{ "OrderId":"201", "Email":"example@test.com", "ProductId":"X201" }' `
    -Headers @{ "Content-Type"="application/json" }
Gets a 401 unauthorized

# example 3 - using the x-functions-key header
iwr -Method POST `
    -Uri "https://pluralsightfuncsarm.azurewebsites.net/api/OnPaymentReceived" `
    -Body '{ "OrderId":"202", "Email":"example@test.com", "ProductId":"X202" }' `
    -Headers @{ "Content-Type"="application/json"
                "x-functions-key"="NEq9oTOPsjdRQz4/Y63X9128BOoHsozr8/J3uws59X3neC40e7RVPg==" }

# example 4 - calling after "renewing" the key will result in 401 unauthorized again
iwr -Method POST `
    -Uri "https://pluralsightfuncsarm.azurewebsites.net/api/OnPaymentReceived" `
    -Body '{ "OrderId":"203", "Email":"example@test.com", "ProductId":"X203" }' `
    -Headers @{ "Content-Type"="application/json"
                "x-functions-key"="NEq9oTOPsjdRQz4/Y63X9128BOoHsozr8/J3uws59X3neC40e7RVPg==" }