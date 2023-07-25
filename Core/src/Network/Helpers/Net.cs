using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network {
    public static class Net {
        /// <summary>
        /// The root net attribute.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
        public abstract class NetAttribute : Attribute {
            /// <summary>
            /// Runs any necessary code for the attribute.
            /// </summary>
            public virtual void OnHandleBegin() { }

            /// <summary>
            /// Returns true if the message handler should immediately stop.
            /// </summary>
            /// <returns></returns>
            public virtual bool StopHandling() => false;

            /// <summary>
            /// Returns true if this attribute can be hooked with HookComplete.
            /// </summary>
            /// <returns></returns>
            public virtual bool IsAwaitable() => false;

            /// <summary>
            /// Registers this action so that it will be called when the message handling can continue.
            /// </summary>
            /// <param name="action"></param>
            public virtual void HookComplete(Action action) => action.Invoke();
        }

        /// <summary>
        /// Waits to handle any recieved messages until the scene has finished loading.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
        public class DelayWhileLoading : NetAttribute {
            public override bool IsAwaitable() => true;

            public override void HookComplete(Action action) {
                FusionSceneManager.HookOnLevelLoad(action);
            }
        }

        /// <summary>
        /// Waits to handle any recieved messages until the server's target scene has finished loading.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
        public class DelayWhileTargetLoading : NetAttribute {
            public override bool IsAwaitable() => true;

            public override void HookComplete(Action action) {
                FusionSceneManager.HookOnTargetLevelLoad(action);
            }
        }

        /// <summary>
        /// Skips handling a message if the game is currently loading.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
        public class SkipHandleWhileLoading : NetAttribute {
            public override bool StopHandling() {
                return FusionSceneManager.IsLoading();
            }
        }
    }
}
