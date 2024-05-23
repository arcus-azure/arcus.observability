// Define the location for the deployment of the components.
param location string

// Define the name of the Application Insights component.
param appInsightsName string

targetScope='subscription'

resource resourceGroup 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: 'arcus-observability-dev-we-rg'
  location: location
}

module workspace 'br/public:avm/res/operational-insights/workspace:0.3.4' = {
  name: 'workspaceDeployment'
  scope: resourceGroup
  params: {
    name: 'arcus-observability-dev-we-workspace'
    location: location
  }
}

module component 'br/public:avm/res/insights/component:0.3.0' = {
  name: 'componentDeployment'
  scope: resourceGroup
  params: {
    name: appInsightsName
    workspaceResourceId: workspace.outputs.resourceId
    location: location
  }
}

output ApplicationInsights_ApplicationId string = component.outputs.applicationId
output ApplicationInsights_ConnectionString string = component.outputs.connectionString
