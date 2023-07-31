# Create the ARM Group (and the ACR and app service instances) 
az deployment group create --resource-group <YourResourceGroup> --template-file ./acr.bicep --query properties.outputs.acrLoginServer.value --parameters acrName=appframeukwacr

# Get the admin username
az acr credential show --name appframeukwacr --query "username"

# Get the admin password
az acr credential show --nameappframeukwacr --query "passwords[0].value"

# Login to the ACR
az acr login --name appframeukwacr
cd /AppFrame/Services/TableServiceAPI

# Build and push the table service
docker build -t appframeukwacr.azurecr.io/tableremoteservice:v1
docker push appframeukwacr.azurecr.io/tableremoteservice:v1

# Build and push the replica service
cd ../ReplicationTargetAPI
docker build -t appframeukwacr.azurecr.io/replicationtargetservice:v1
docker push appframeukwacr.azurecr.io/replicationtargetservice:v1

