using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Entities;

public interface IEntityUpdatable
{
    void OnEntityUpdate(float deltaTime);
}

public interface IEntityFixedUpdatable
{
    void OnEntityFixedUpdate(float deltaTime);
}

public interface IEntityLateUpdatable
{
    void OnEntityLateUpdate(float deltaTime);
}