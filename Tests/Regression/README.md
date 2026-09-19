Run with the .NET 6 SDK or a newer SDK with the .NET 6 runtime installed:

```sh
dotnet run --project Tests/Regression/Regression.csproj -c Release
```

These checks compile the production serialization, entity ID, updater, download, proxy matchmaking, and audio queue code. Game and service APIs use test doubles. No game installation, account, or live network connection is needed.

This does not replace a full solution build or PCVR and Quest multiplayer testing.
