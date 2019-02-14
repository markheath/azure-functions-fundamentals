# create a resource group
$resourceGroup = "pluralsightfuncsarm"
$location = "westeurope"
az group create -n $resourceGroup -l $location

# choose a name for our function app
$functionAppName = "pluralsightfuncsarm"

# to deploy our function app
az group deployment create -g $resourceGroup --template-file azuredeploy.json `
    --parameters appName=$functionAppName

# see what's in the resource group we just created
az resource list -g $resourceGroup -o table

# check the app settings were configured correctly
az functionapp config appsettings list `
    -n $functionAppName -g $resourceGroup -o table

# to clean up when we're done
az group delete -n $resourceGroup --no-wait -y
