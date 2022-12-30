using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using SLZ.Bonelab;
using SLZ.Vehicle;

using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Syncables;

namespace LabFusion.Data
{
    public static class MagmaGateData
    {
        public static GameControl_MagmaGate GameController;

        public static void OnCacheMagmaGateInfo()
        {
            GameController = GameObject.FindObjectOfType<GameControl_MagmaGate>(true);
        }
    }
}
