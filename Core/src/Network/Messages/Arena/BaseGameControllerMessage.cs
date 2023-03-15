using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Patching;

namespace LabFusion.Network
{
    public enum BaseGameControllerType
    {
        UNKNOWN = 0,
        BeginSession = 1,
        EndSession = 2,
    }

    public class BaseGameControllerData : IFusionSerializable, IDisposable
    {
        public BaseGameControllerType type;

        public void Serialize(FusionWriter writer)
        {
            writer.Write((byte)type);
        }

        public void Deserialize(FusionReader reader)
        {
            type = (BaseGameControllerType)reader.ReadByte();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static BaseGameControllerData Create(BaseGameControllerType type)
        {
            return new BaseGameControllerData()
            {
                type = type,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class BaseGameControllerMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.BaseGameController;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<BaseGameControllerData>())
                {
                    BaseGameControllerPatches.IgnorePatches = true;

                    // We ONLY handle this for clients, this message should only ever be sent by the server!
                    if (!NetworkInfo.IsServer && GameControllerData.HasGameController)
                    {
                        switch (data.type)
                        {
                            default:
                            case BaseGameControllerType.UNKNOWN:
                                break;
                            case BaseGameControllerType.BeginSession:
                                GameControllerData.GameController.BeginSession();
                                break;
                            case BaseGameControllerType.EndSession:
                                GameControllerData.GameController.EndSession();
                                break;
                        }
                    }

                    BaseGameControllerPatches.IgnorePatches = false;
                }
            }
        }
    }
}
