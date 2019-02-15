# how to use the Azure Functions Core Tools to publish:
# n.b. by default this will use "run from package" zip
# deployment and put the zip in blob storage
$appName = "myfunctionapp"
func azure functionapp publish $appName

# how to use the Azure CLI to publish a zip file
# n.b. if the function app already has
# WEBSITE_RUN_FROM_PACKAGE set with a value of 1
# then this will upload the zip into D:\home\data\SitePackages 
# and update packagename.txt to point to it
# Otherwise the zip will be unpacked directly to
# D:\home\site\wwwroot
$resourceGroupName = "myresourcegroup"
az functionapp deployment source config-zip `
      -g $resourceGroupName -n $appName `
      --src "publish.zip"

