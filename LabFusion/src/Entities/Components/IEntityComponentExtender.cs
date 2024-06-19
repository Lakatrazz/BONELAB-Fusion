using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Entities;

public interface IEntityComponentExtender : IEntityExtender
{
    bool TryRegister(NetworkEntity networkEntity, params GameObject[] parents);

    void Unregister();
}
