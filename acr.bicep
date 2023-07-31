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

output acrLoginServer string = acr.properties.loginServer
