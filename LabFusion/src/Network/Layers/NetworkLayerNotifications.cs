using LabFusion.UI.Popups;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network;

public static class NetworkLayerNotifications
{   
	public const string NotificationTag = "NetworkLayer";

	public static void SendLoggingInNotification()
	{
		Notifier.Cancel(NotificationTag);

		Notifier.Send(new Notification()
		{
			Title = "Please Wait!",
			Message = $"Logging in to {NetworkLayerDeterminer.LoadedLayer.Platform}...",
			Tag = NotificationTag,
			SaveToMenu = false,
			ShowPopup = true,
			Type = NotificationType.INFORMATION,
		});
	}
}
