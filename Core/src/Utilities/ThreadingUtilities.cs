using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnhollowerBaseLib;

namespace LabFusion.Utilities {
    public static class ThreadingUtilities {
        public static readonly ConcurrentQueue<Action> SynchronousEvents = new();

        internal static void Internal_OnUpdate() {
            if (SynchronousEvents.Count > 0) {
                while (SynchronousEvents.TryDequeue(out Action action)) {
                    try {
                        action();
                    }
                    catch (Exception e) {
                        FusionLogger.LogException("executing CompleteEvent", e);
                    }
                }
            }
        }

        /// <summary>
        /// Enqueues a complete event for a thread to be ran synchronously.
        /// </summary>
        /// <param name="action"></param>
        public static void RunSynchronously(Action action) {
            if (action != null)
                SynchronousEvents.Enqueue(action);
        }

        /// <summary>
        /// Registers the active domain in il2cpp in order to prevent GC errors.
        /// <para>This should be called when the current thread is changed and you want to use IL2 objects.</para>
        /// </summary>
        public static void IL2PrepareThread(out IntPtr thread) {
            thread = IL2CPP.il2cpp_thread_attach(IL2CPP.il2cpp_domain_get());
        }

        /// <summary>
        /// Registers the active domain in il2cpp in order to prevent GC errors.
        /// <para>This should be called when the current thread is changed and you want to use IL2 objects.</para>
        /// </summary>
        public static void IL2PrepareThread()
        {
            IL2CPP.il2cpp_thread_attach(IL2CPP.il2cpp_domain_get());
        }

        /// <summary>
        /// Detaches the active domain from il2cpp.
        /// <para>This should be called in a thread after you are done using IL2 objects.</para>
        /// </summary>
        public static void IL2DetachThread(in IntPtr thread) {
            IL2CPP.il2cpp_thread_detach(thread);
        }
    }
}
