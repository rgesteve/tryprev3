@description('The name of you Virtual Machine.')
param vmname    string
param vmuser    string
// @secure()
// param vmpass    string
@description('Unique DNS Name for the Public IP used to access the Virtual Machine.')
param dnsLabelPrefix string = toLower('${vmname}-${uniqueString(resourceGroup().id)}')
@description('SSH Key for the Virtual Machine.')
@secure()
param publickey string

// This issues a warning
var location = resourceGroup().location

resource nsg 'Microsoft.Network/networkSecurityGroups@2020-07-01' = {
  name      : '${vmname}-nsg'
  location  : location
  properties: {
    securityRules: [
      {
        name: 'allow-ssh'
        properties: {
          priority                 : 110
          protocol                 : 'Tcp'
          sourcePortRange          : '*'
          destinationPortRange     : '22'
          sourceAddressPrefix      : '*'
          destinationAddressPrefix : '*'
          access                   : 'Allow'
          direction                : 'Inbound'
        }
      }
    ]
  }
}

resource vnet 'Microsoft.Network/virtualNetworks@2020-07-01' = {
  name: '${vmname}-vnet'
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.0.0.0/16'
      ]
    }
  }
}

resource subnet 'Microsoft.Network/virtualNetworks/subnets@2020-07-01' = {
  parent: vnet
  name: '${vmname}-subnet'
  properties: {
    addressPrefix: '10.0.0.0/24'
    privateEndpointNetworkPolicies: 'Enabled'
    privateLinkServiceNetworkPolicies: 'Enabled'
  }
}

resource pip 'Microsoft.Network/publicIPAddresses@2020-07-01' = {
  name: '${vmname}-pip'
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    dnsSettings: {
      domainNameLabel: dnsLabelPrefix
    }
  }
}

resource nic 'Microsoft.Network/networkInterfaces@2020-07-01' = {
  name: '${vmname}-nic'
  location: location
  properties: {
    ipConfigurations: [
        {
          name: 'ifcfg'
          properties: {
            publicIPAddress: {
              id: pip.id
            }
            subnet         : {
              id: subnet.id
            }
            privateIPAllocationMethod: 'Dynamic'
          }
        }
    ]
  }
}

resource vm 'Microsoft.Compute/virtualMachines@2020-12-01' = {
  name      : vmname
  location  : location
  properties: {
    hardwareProfile: {
      //vmSize: 'Standard_B2ms'
      vmSize: 'Standard_D8_v5'
    }
    storageProfile: {
      imageReference: {
        publisher: 'Canonical'
        offer: '0001-com-ubuntu-server-focal'
        sku: '20_04-lts'
        version: 'latest'
      }
      osDisk: {
        name: '${vmname}-osdisk'
        createOption: 'FromImage'
        caching: 'ReadWrite'
        managedDisk: {
          // storageAccountType: 'Premium_LRS'  // Standard_D8_v5 does not support premium storage
          storageAccountType: 'Standard_LRS'
        }
        diskSizeGB: 64
      }
    }
    osProfile: {
      computerName : vmname
      adminUsername: vmuser
      //adminPassword: vmpass
      // TODO: Re-enable SSH authentication
      linuxConfiguration: {
        // disablePasswordAuthentication: true
        ssh: {
          publicKeys: [
            {
              path: '/home/${vmuser}/.ssh/authorized_keys'
              keyData: publickey
            }
          ]
        }
      }
      customData: base64(loadTextContent('cloud-init.yaml'))
    }
    networkProfile: {
      networkInterfaces: [
        {
          id: nic.id
        }
      ]
    }
  }
}

output adminUsername string = vmuser
output hostname string = pip.properties.dnsSettings.fqdn
output sshCommand string = 'ssh ${vmuser}@${pip.properties.dnsSettings.fqdn}'
