param acrName string
param acrSku string = 'Standard'
param webAppSku string = 'B1'
param location1 string = 'UK West'
param location2 string = 'Australia West'
param location3 string = 'US West 2'
param location4 string = 'North Europe'

resource acr 'Microsoft.ContainerRegistry/registries@2021-06-01-preview' = {
  name: acrName
  location: location1
  sku: {
    name: acrSku
  }
  properties: {
    adminUserEnabled: true
  }
}

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
  dependsOn: [
    acr
  ]
}]

resource webApps 'Microsoft.Web/sites@2021-02-01' = [for (plan, index) in appServicePlans: {
  name: '${plan.name}TableServiceAPI'
  location: plan.location
  properties: {
    serverFarmId: asp[index].id
    siteConfig: {
      linuxFxVersion: 'DOCKER|${acr.loginServer}/tableremoteservice:v1'
      appSettings: [
        {
          name: 'WEBSITES_ENABLE_APP_SERVICE_STORAGE'
          value: 'false'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_URL'
          value: 'https://${acr.loginServer}'
        }
      ]
    }
  }
  dependsOn: [
    acr
    asp
  ]
}]

resource webApps2 'Microsoft.Web/sites@2021-02-01' = [for (plan, index) in appServicePlans: {
  name: '${plan.name}ReplicaTargetAPI'
  location: plan.location
  properties: {
    serverFarmId: asp[index].id
    siteConfig: {
      linuxFxVersion: 'DOCKER|${acr.loginServer}/replicatargetservice:v1'
      appSettings: [
        {
          name: 'WEBSITES_ENABLE_APP_SERVICE_STORAGE'
          value: 'false'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_URL'
          value: 'https://${acr.loginServer}'
        }
      ]
    }
  }
  dependsOn: [
    acr
    asp
  ]
}]
