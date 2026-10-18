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
- Save the client policy and then create a client under the "Clients" section with the newly created client policy.
- Go to the "SDK Download & Credentials" tab and paste all the credentials (ProductName, ProductId, ClientId, ClientSecret, SandboxId, DeploymentId) into the corresponding fields in EOSPlatform.cs.
- Done