using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Network;

namespace LabFusion.Data
{
    public interface IFusionWritable {
        void Serialize(FusionWriter writer);
    }

    public interface IFusionReadable {
        void Deserialize(FusionReader reader);
    }

    public interface IFusionSerializable : IFusionWritable, IFusionReadable {
    }
}
