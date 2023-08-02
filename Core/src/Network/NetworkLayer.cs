﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using BoneLib.BoneMenu.Elements;
using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;

namespace LabFusion.Network
{
    /// <summary>
    /// Privacy type for a server.
    /// </summary>
    public enum ServerPrivacy {
        PUBLIC = 0,
        PRIVATE = 1,
        FRIENDS_ONLY = 2,
        LOCKED = 3,
    }

    /// <summary>
    /// The foundational class for a server's networking system.
    /// </summary>
    public abstract class NetworkLayer {
        private Type _type;
        private bool _hasType;

        /// <summary>
        /// The Type of this NetworkLayer.
        /// </summary>
        internal Type Type { 
            get {
                if (!_hasType) {
                    _type = GetType();
                    _hasType = true;
                }
                return _type;
            } 
        }

        /// <summary>
        /// The Title of this NetworkLayer. Used for saving preferences.
        /// </summary>
        internal virtual string Title => Type.AssemblyQualifiedName;

        /// <summary>
        /// Returns true if this layer is hosting a server.
        /// </summary>
        internal virtual bool IsServer => false;

        /// <summary>
        /// Returns true if this layer is a client inside of a server (still returns true if this is the host!)
        /// </summary>
        internal virtual bool IsClient => false;

        /// <summary>
        /// Returns true if the networking solution allows the server to send messages to the host (Actual Server Logic vs P2P).
        /// </summary>
        internal virtual bool ServerCanSendToHost => true;

        /// <summary>
        /// Returns the current active lobby.
        /// </summary>
        internal virtual INetworkLobby CurrentLobby => null;

        /// <summary>
        /// Returns the used voice manager.
        /// </summary>
        internal virtual IVoiceManager VoiceManager => null;

        /// <summary>
        /// Returns true if this NetworkLayer is supported on the current platform.
        /// </summary>
        /// <returns></returns>
        internal abstract bool CheckSupported();

        /// <summary>
        /// Returns true if this NetworkLayer is valid and able to be ran.
        /// </summary>
        /// <returns></returns>
        internal abstract bool CheckValidation();

        /// <summary>
        /// Returns a fallback layer if it exists in the event this layer fails.
        /// </summary>
        /// <param name="fallback"></param>
        /// <returns></returns>
        internal virtual bool TryGetFallback(out NetworkLayer fallback) {
            fallback = null;
            return false;
        }

        /// <summary>
        /// Starts the server.
        /// </summary>
        internal abstract void StartServer();

        /// <summary>
        /// Disconnects the client from the connection and/or server.
        /// </summary>
        internal abstract void Disconnect(string reason = "");

        /// <summary>
        /// Returns the username of the player with id userId.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        internal virtual string GetUsername(ulong userId) => "Unknown";

        /// <summary>
        /// Returns true if this is a friend (ex. steam friends).
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        internal virtual bool IsFriend(ulong userId) => false;

        /// <summary>
        /// Sends the message to the specified user if this is a server.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        internal virtual void SendFromServer(byte userId, NetworkChannel channel, FusionMessage message) { }

        /// <summary>
        /// Sends the message to the specified user if this is a server.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        internal virtual void SendFromServer(ulong userId, NetworkChannel channel, FusionMessage message) { }

        /// <summary>
        /// Sends the message to the dedicated server.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        internal virtual void SendToServer(NetworkChannel channel, FusionMessage message) { }

        /// <summary>
        /// Sends the message to the server if this is a client. Sends to all clients if this is a server.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        internal virtual void BroadcastMessage(NetworkChannel channel, FusionMessage message) { }

        /// <summary>
        /// If this is a server, sends this message back to all users except for the provided id.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        internal virtual void BroadcastMessageExcept(byte userId, NetworkChannel channel, FusionMessage message, bool ignoreHost = true) {
            for (var i = 0; i < PlayerIdManager.PlayerIds.Count; i++) {
                var id = PlayerIdManager.PlayerIds[i];

                if (id.SmallId != userId && (id.SmallId != 0 || !ignoreHost))
                    SendFromServer(id.SmallId, channel, message);
            }
        }

        /// <summary>
        /// If this is a server, sends this message back to all users except for the provided id.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        internal virtual void BroadcastMessageExcept(ulong userId, NetworkChannel channel, FusionMessage message, bool ignoreHost = true)
        {
            for (var i = 0; i < PlayerIdManager.PlayerIds.Count; i++) {
                var id = PlayerIdManager.PlayerIds[i];
                if (id.LongId != userId && (id.SmallId != 0 || !ignoreHost))
                    SendFromServer(id.SmallId, channel, message);
            }
        }

        internal abstract void OnInitializeLayer();

        internal virtual void OnLateInitializeLayer() { }

        internal abstract void OnCleanupLayer();

        internal virtual void OnUpdateLayer() { }

        internal virtual void OnLateUpdateLayer() { }

        internal virtual void OnGUILayer() { }

        internal virtual void OnVoiceChatUpdate() { }

        internal virtual void OnVoiceBytesReceived(PlayerId id, byte[] bytes) { }

        internal virtual void OnUserJoin(PlayerId id) { }

        internal virtual void OnSetupBoneMenu(MenuCategory category) { }

        public static void RegisterLayersFromAssembly(Assembly targetAssembly)
        {
            if (targetAssembly == null) throw new NullReferenceException("Can't register from a null assembly!");

            FusionLogger.Log($"Populating NetworkLayer list from {targetAssembly.GetName().Name}!");

            AssemblyUtilities.LoadAllValid<NetworkLayer>(targetAssembly, RegisterLayer);
        }

        public static void RegisterLayer<T>() where T : NetworkLayer => RegisterLayer(typeof(T));

        private static void RegisterLayer(Type type)
        {
            NetworkLayer layer = Activator.CreateInstance(type) as NetworkLayer;

            if (string.IsNullOrWhiteSpace(layer.Title))
            {
                FusionLogger.Warn($"Didn't register {type.Name} because its Title was invalid!");
            }
            else
            {
                if (LayerLookup.ContainsKey(layer.Title)) throw new Exception($"{type.Name} has the same Title as {LayerLookup[layer.Title].GetType().Name}, we can't replace layers!");

                FusionLogger.Log($"Registered {type.Name}");

                Layers.Add(layer);
                LayerLookup.Add(layer.Title, layer);

                if (layer.CheckSupported())
                    SupportedLayers.Add(layer);
            }
        }

        public static bool TryGetLayer<T>(out T layer) where T : NetworkLayer
        {
            layer = (T)Layers.Find((l) => l.Type == typeof(T));
            return layer != null;
        }

        public static T GetLayer<T>() where T : NetworkLayer
        {
            return (T)Layers.Find((l) => l.Type == typeof(T));
        }

        public static readonly List<NetworkLayer> Layers = new();
        public static readonly FusionDictionary<string, NetworkLayer> LayerLookup = new();
        public static readonly List<NetworkLayer> SupportedLayers = new();
    }
}
