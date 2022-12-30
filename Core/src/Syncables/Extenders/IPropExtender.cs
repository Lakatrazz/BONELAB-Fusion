using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Grabbables;
using LabFusion.Network;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Syncables {
    public interface IPropExtender {
         bool ValidateExtender(PropSyncable syncable);
        
         void OnCleanup();
    }
}
