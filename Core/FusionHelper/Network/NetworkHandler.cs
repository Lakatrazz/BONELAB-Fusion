using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WatsonWebserver;
using HttpMethod = WatsonWebserver.HttpMethod;

namespace FusionHelper.WebSocket
{
    internal class NetworkHandler
    {
        public static void Init()
        {
            Server server = new("127.0.0.1", 9000, false, DefaultRoute);
            server.Start();

            Console.WriteLine("Initialized websocket server at localhost:9000");

            server.Routes.Static.Add(HttpMethod.GET, "/steamid", GetSteamIDRoute);
        }

        private static async Task DefaultRoute(HttpContext ctx)
        {
            ctx.Response.StatusCode = 200;
            await ctx.Response.Send("I am alive!");
        }

        private static async Task GetSteamIDRoute(HttpContext ctx)
        {
            await ctx.Response.Send(BitConverter.GetBytes(SteamClient.SteamId.Value));
        }
    }
}
