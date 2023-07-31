# Create the ARM Group (and the ACR and app service instances) 
az deployment group create --resource-group appframe --template-file ./acr.bicep --parameters acrName=appframeukwacr

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

