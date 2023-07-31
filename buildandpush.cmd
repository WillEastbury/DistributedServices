az deployment group create --resource-group appframe> --template-file ./acr.bicep --parameters acrName=appframeukwacr
az acr login --name appframeukwacr
docker build -t TableService
az acr login --name appframeukwacr

docker tag TableService %%acrLoginServer%%/%%service%%:%%tag%%
docker push %%acrLoginServer%%/%%service%%:%%tag%%

