# how to use the Azure Functions Core Tools to generate a new
# function app with a Docker file:
func init --docker

# how to build a Docker image of a function app
docker build -t pluralsightfuncs:v1 .

# how to run a Docker container locally, with an environment
# variable, and an exposed port
docker run -d -e AppSettingName=Value -p 8080:80 dockerfuncs:v1