using System.Collections;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.Platform;
using UnityEngine;

namespace LabFusion.Network.EpicGames;

internal class EOSAuthManager
{
    internal EOSAuthManager()
    {
        
    }
    
    internal static IEnumerator Login(System.Action<bool> onComplete)
    {
        void CreateDeviceID()
        {
            var connect = EOSManager.ConnectInterface;
            
            var createDeviceIdOptions = new CreateDeviceIdOptions
            {
                DeviceModel = SystemInfo.deviceModel,
            };
        }
    }
}