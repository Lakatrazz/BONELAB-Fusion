using LabFusion.Marrow.Proxies;
using LabFusion.Network;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Menu;

public static class MenuLocation
{
    public static MenuPage LocationPage { get; private set; }

    public static void OnInitializeMelon()
    {
        MultiplayerHooking.OnStartServer += OnConnect;
        MultiplayerHooking.OnJoinServer += OnConnect;
        MultiplayerHooking.OnDisconnect += OnDisconnect;
    }

    private static void OnConnect()
    {
        if (LocationPage == null)
        {
            return;
        }

        LocationPage.DefaultPageIndex = 1;
        LocationPage.SelectSubPage(1);
    }

    private static void OnDisconnect()
    {
        if (LocationPage == null)
        {
            return;
        }

        LocationPage.DefaultPageIndex = 0;
        LocationPage.SelectSubPage(0);
    }

    public static void PopulateLocation(GameObject locationPage)
    {
        LocationPage = locationPage.GetComponent<MenuPage>();

        // No Server subpage
        var noServerPage = locationPage.transform.Find("subPage_NoServer");

        var createServerButton = noServerPage.Find("button_CreateServer").GetComponent<FunctionElement>();

        createServerButton.Title = "Create Server";

        createServerButton.OnPressed += NetworkHelper.StartServer;

        // Server subpage
        var serverPage = locationPage.transform.Find("subPage_Server");

        var disconnectButton = serverPage.Find("button_Disconnect").GetComponent<FunctionElement>();

        disconnectButton.Title = "Disconnect";

        disconnectButton.OnPressed += () => { NetworkHelper.Disconnect(); };

        // Update server status
        if (NetworkInfo.HasServer)
        {
            OnConnect();
        }
        else
        {
            OnDisconnect();
        }
    }
}