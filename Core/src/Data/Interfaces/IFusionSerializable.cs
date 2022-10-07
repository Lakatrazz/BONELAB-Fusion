using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Network;

namespace LabFusion.Data
{
    public interface IFusionSerializable
    {
        void Deserialize(FusionReader reader);

        void Serialize(FusionWriter writer);
    }
}
