using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network
{
    public abstract class NetworkLayer {
        public abstract void OnInitializeLayer();

        public virtual void OnLateInitializeLayer() { }

        public abstract void OnCleanupLayer();

        public virtual void OnUpdateLayer() { }

        public virtual void OnLateUpdateLayer() { }

        public virtual void OnGUILayer() { }
    }
}
