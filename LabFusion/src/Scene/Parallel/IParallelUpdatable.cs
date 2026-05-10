using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Scene;

public interface IParallelFixedUpdatable
{
    void OnPreParallelFixedUpdate(float deltaTime);
    void OnParallelFixedUpdate(float deltaTime);
    void OnPostParallelFixedUpdate(float deltaTime);
}
