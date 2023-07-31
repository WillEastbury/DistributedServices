docker build -t %%service%%:%%tag .
az acr login --name %%acrname%%
docker tag %%service%%:%%tag%% %%acrLoginServer%%/%%service%%:%%tag%%
docker push %%acrLoginServer%%/%%service%%:%%tag%%