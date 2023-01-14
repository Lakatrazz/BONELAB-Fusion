using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Utilities {
    // SafeActions from BoneLib, but using generic delegates instead
    internal static class SafeEvents {
        internal static void InvokeSafe<T>(this T action) where T : Delegate
        {
            if (action == null)
            {
                return;
            }

            Delegate[] invocationList = action.GetInvocationList();
            foreach (Delegate @delegate in invocationList)
            {
                try
                {
                    Action action2 = (Action)@delegate;
                    action2();
                }
                catch (Exception ex)
                {
                    FusionLogger.Error("Exception while invoking hook callback!");
                    FusionLogger.Error(ex.ToString());
                }
            }
        }

        internal static void InvokeSafe<T, T1>(this T action, T1 param) where T : Delegate
        {
            if (action == null)
            {
                return;
            }

            Delegate[] invocationList = action.GetInvocationList();
            foreach (Delegate @delegate in invocationList)
            {
                try
                {
                    Action<T1> action2 = (Action<T1>)@delegate;
                    action2(param);
                }
                catch (Exception ex)
                {
                    FusionLogger.Error("Exception while invoking hook callback!");
                    FusionLogger.Error(ex.ToString());
                }
            }
        }

        internal static void InvokeSafe<T, T1, T2>(this T action, T1 param1, T2 param2) where T : Delegate
        {
            if (action == null) {
                return;
            }

            Delegate[] invocationList = action.GetInvocationList();
            foreach (Delegate @delegate in invocationList)
            {
                try
                {
                    Action<T1, T2> action2 = (Action<T1, T2>)@delegate;
                    action2(param1, param2);
                }
                catch (Exception ex)
                {
                    FusionLogger.Error("Exception while invoking hook callback!");
                    FusionLogger.Error(ex.ToString());
                }
            }
        }
    }
}
