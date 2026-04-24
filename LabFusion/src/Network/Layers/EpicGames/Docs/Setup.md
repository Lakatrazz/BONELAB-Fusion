# Epic Games Networking Layer Setup Guide

### Setting up the Epic Games Developer Portal
- Go to https://dev.epicgames.com/en-US and click on "Dev portal" in the top right corner.
- Sign in with your Epic Games account or create a new one if you don't have one.
- Once signed in, create an organization if you don't have one already.
- After creating an organization, create a new product.
- Fill in the required details for the product and save it.
- Once you have created the product, navigate to "Product Settings" and then to the "Clients" section.
- Create a "Client Policy" with the following settings:
  - Client policy type : Peer2Peer
  - Features : Lobbies, Notifications, Player Reports
- Save the client policy and then create a client under the "Clients" section with the newly created client policy.
- Go to the "SDK Download & Credentials" tab and paste all the credentials (ProductName, ProductId, ClientId, ClientSecret, SandboxId, DeploymentId) into the corresponding fields in EOSCredentials.cs.
- Go to the "Identity Providers" tab and add a new provider with the following settings:
  - Provider Type: Steam
  - Description : Steam Provider
  - AppId : 1592190
- Save the provider and then go to the "Sandboxes" tab.
- Click on "Identity Providers" next to the sandbox you used for EOSCredentials and add the newly created Steam provider.
- Done