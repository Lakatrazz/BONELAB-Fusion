using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Grabbables;
using LabFusion.Syncables;
using LabFusion.Patching;

using SLZ;
using SLZ.Interaction;
using SLZ.Props.Weapons;
using LabFusion.Exceptions;

namespace LabFusion.Network
{
    public class MineDiveCartData : IFusionSerializable, IDisposable
    {
        public int amount;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(amount);
        }

        public void Deserialize(FusionReader reader)
        {
            amount = reader.ReadInt32();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static MineDiveCartData Create(int amount)
        {
            return new MineDiveCartData()
            {
                amount = amount,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class MineDiveCartMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.MineDiveCart;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using FusionReader reader = FusionReader.Create(bytes);
            using var data = reader.ReadFusionSerializable<MineDiveCartData>();

            if (!NetworkInfo.IsServer) {
                MineDiveData.CreateExtraCarts(data.amount);
            }
            else
                throw new ExpectedClientException();
        }
    }
}
