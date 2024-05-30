// Define the location for the deployment of the components.
param location string

// Define the name of the resource group where the components will be deployed.
param resourceGroupName string

// Define the name of the Application Insights component.
param appInsightsName string

// Define the name of the Key Vault.
param keyVaultName string

// Define the Service Principal ID that needs access full access to the deployed resource group.
param servicePrincipalId string

targetScope='subscription'

module resourceGroup 'br/public:avm/res/resources/resource-group:0.2.3' = {
  name: 'resourceGroupDeployment'
  params: {
    name: resourceGroupName
    location: location
    roleAssignments: [
      {
        principalId: servicePrincipalId
        roleDefinitionIdOrName: 'Owner'
      }
    ]
  }
}

resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' existing = {
  name: resourceGroupName
}

module workspace 'br/public:avm/res/operational-insights/workspace:0.3.4' = {
  name: 'workspaceDeployment'
  scope: rg
  params: {
    name: 'arcus-observability-dev-we-workspace'
    location: location
  }
}

module component 'br/public:avm/res/insights/component:0.3.0' = {
  name: 'componentDeployment'
  scope: rg
  params: {
    name: appInsightsName
    workspaceResourceId: workspace.outputs.resourceId
    location: location
  }
}

module vault 'br/public:avm/res/key-vault/vault:0.6.1' = {
  name: 'vaultDeployment'
  scope: rg
  params: {
    name: keyVaultName
    location: location
    roleAssignments: [
      {
        principalId: servicePrincipalId
        roleDefinitionIdOrName: 'Key Vault Secrets Contributor'
      }
    ]
    secrets: [
      {
        name: 'ApplicationInsights_ConnectionString'
        value: component.outputs.connectionString
      }
    ]
  }
}

output ApplicationInsights_WorkspaceId string = workspace.outputs.resourceId
output ApplicationInsights_ApplicationId string = component.outputs.applicationId
output KeyVault_Uri string = vault.outputs.uri
