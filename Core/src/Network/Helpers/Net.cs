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
            /// Returns true if this attribute can be awaited with CanContinue.
            /// </summary>
            /// <returns></returns>
            public virtual bool IsAwaitable() => false;

            /// <summary>
            /// Returns true if the message handler can continue while awaiting this attribute.
            /// </summary>
            /// <returns></returns>
            public virtual bool CanContinue() => true;
        }

        /// <summary>
        /// Waits to handle any recieved messages until the scene has finished loading.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
        public class DelayWhileLoading : NetAttribute {
            public override bool IsAwaitable() => true;

            public override bool CanContinue() {
                return !FusionSceneManager.IsLoading();
            }
        }

        /// <summary>
        /// Waits to handle any recieved messages until the server's target scene has finished loading.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
        public class DelayWhileTargetLoading : NetAttribute {
            public override bool IsAwaitable() => true;

            public override bool CanContinue() {
                return FusionSceneManager.HasTargetLoaded();
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
