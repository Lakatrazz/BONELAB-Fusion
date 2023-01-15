using LabFusion.Data;
using LabFusion.Exceptions;
using LabFusion.Extensions;
using LabFusion.Representation;
using LabFusion.Senders;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Network
{
    public class TimeScaleMessageData : IFusionSerializable, IDisposable
    {
        public float timeScale;

        public static TimeScaleMessageData Create() {
            return new TimeScaleMessageData() {
                timeScale = Time.timeScale,
            };
        }

        public void Serialize(FusionWriter writer)
        {
            writer.Write(timeScale);
        }

        public void Deserialize(FusionReader reader)
        {
            timeScale = reader.ReadSingle();
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }
    }

    [Net.SkipHandleWhileLoading]
    public class TimeScaleMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.TimeScale;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            if (!NetworkInfo.CurrentNetworkLayer.IsServer)
            {
                using (var reader = FusionReader.Create(bytes))
                {
                    using (var data = reader.ReadFusionSerializable<TimeScaleMessageData>())
                    {
                        TimeScaleSender.ReceivedTimeScale = data.timeScale;
                    }
                }
            }
            else
                throw new ExpectedClientException();
        }
    }
}
