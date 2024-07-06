﻿using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Player;

using System.Xml.Linq;

namespace LabFusion.XML;

public sealed class PlayerList
{
    public struct PlayerInfo
    {
        public string username;

        public PlayerInfo(PlayerId id)
        {
            username = id.Metadata.GetMetadata(MetadataHelper.UsernameKey);
        }

        public PlayerInfo(XElement element)
        {
            element.TryGetAttribute(nameof(username), out username);
        }

        public void WriteElement(XElement element)
        {
            element.SetAttributeValue(nameof(username), username);
        }
    }

    public PlayerInfo[] players;

    public void ReadPlayerList()
    {
        // Create player info array from all players
        players = new PlayerInfo[PlayerIdManager.PlayerCount];
        int index = 0;

        foreach (var player in PlayerIdManager.PlayerIds)
        {
            players[index++] = new PlayerInfo(player);
        }
    }

    public void ReadDocument(XDocument document)
    {
        var descendants = document.Descendants(nameof(PlayerInfo));
        players = new PlayerInfo[descendants.Count()];

        for (var i = 0; i < players.Length; i++)
        {
            players[i] = new PlayerInfo(descendants.ElementAt(i));
        }
    }

    public XDocument WriteDocument()
    {
        var doc = CreateDefault();

        for (var i = 0; i < players.Length; i++)
        {
            XElement entry = new(nameof(PlayerInfo));
            players[i].WriteElement(entry);
            doc.Root.Add(entry);
        }

        return doc;
    }

    private XDocument CreateDefault()
    {
        XDocument document = new();
        document.Add(new XElement("Root"));
        return document;
    }
}