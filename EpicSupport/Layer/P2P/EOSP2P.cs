using Epic.OnlineServices;
using Epic.OnlineServices.P2P;

namespace MarrowFusion.Epic;

internal class EOSP2P : EOSInterface
{
    internal const byte ClientReliableChannel = 1;
    internal const byte ClientUnreliableChannel = 2;
    internal const byte ServerReliableChannel = 3;
    internal const byte ServerUnreliableChannel = 4;
    
    internal EOSRuntime Runtime;
    internal P2PInterface P2PInterface;
    internal ProductUserId LocalUserId;
    
    internal SocketId SocketId { get; } = new() { SocketName = "Marrow Fusion" };

    internal EOSP2PClient Client;
    internal EOSP2PServer Server;
    
    internal PacketFragmenter Fragmenter;
    internal EOSP2PSender Sender;
    internal EOSP2PReceiver Receiver;

    internal EOSP2P(EOSRuntime eosRuntime, P2PInterface p2pInterface, ProductUserId localUserId)
    {
        Runtime = eosRuntime;
        P2PInterface = p2pInterface;
        LocalUserId = localUserId;
        
        Client = new EOSP2PClient(this);
        Server = new EOSP2PServer(this);
        
        Fragmenter = new PacketFragmenter();
        Sender = new EOSP2PSender(this);
        Receiver = new EOSP2PReceiver(this);
    }

    internal override Task<bool> InitializeAsync()
    {
        var relayOptions = new SetRelayControlOptions
        {
            RelayControl = RelayControl.ForceRelays
        };
        
        P2PInterface.SetRelayControl(ref relayOptions);

        return Task.FromResult(true);
    }

    internal override void Tick()
    {
        Receiver.Tick();
    }
}