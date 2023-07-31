param acrLoginServer string = 'appframeukwacr.azurecr.io'
param acrName string = 'appframeukwacr'
param acrUsername string 
param acrPassword string 
param location1 string = 'UK West'
param location2 string = 'Australia Central'
param location3 string = 'West US'
param location4 string = 'North Europe'
param webAppSku string = 'B1'

var locations = [
  location1
  location2
  location3
  location4
]

var appServicePlans = [for location in locations: {
  name: '${acrName}${replace(location, ' ', '')}ASP'
  location: location
  sku: webAppSku
}]

resource asp 'Microsoft.Web/serverfarms@2021-02-01' = [for (plan, index) in appServicePlans: {
  name: plan.name
  location: plan.location
  sku: {
    name: plan.sku
    capacity: 1
  }
  tags: {
    displayName: 'HostingPlan'
  }
  properties: {
    reserved: true // for Linux
  }
}]

resource webApps 'Microsoft.Web/sites@2021-02-01' = [for (plan, index) in appServicePlans: {
  name: '${plan.name}TableServiceAPI'
  location: plan.location
  properties: {
    serverFarmId: asp[index].id
    siteConfig: {
      linuxFxVersion: 'DOCKER|${acrLoginServer}/tableremoteservice:v1'
      appSettings: [
        {
          name: 'WEBSITES_ENABLE_APP_SERVICE_STORAGE'
          value: 'false'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_URL'
          value: 'https://${acrLoginServer}'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_USERNAME'
          value: acrUsername
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_PASSWORD'
          value: acrPassword
        }
      ]
    }
  }
}]

resource webApps2 'Microsoft.Web/sites@2021-02-01' = [for (plan, index) in appServicePlans: {
  name: '${plan.name}ReplicaTargetAPI'
  location: plan.location
  properties: {
    serverFarmId: asp[index].id
    siteConfig: {
      linuxFxVersion: 'DOCKER|${acrLoginServer}/replicatargetservice:v1'
      appSettings: [
        {
          name: 'WEBSITES_ENABLE_APP_SERVICE_STORAGE'
          value: 'false'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_URL'
          value: 'https://${acrLoginServer}'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_USERNAME'
          value: acrUsername
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_PASSWORD'
          value: acrPassword
        }
      ]
    }
  }
}]
