using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Utilities {
    // SafeActions from BoneLib, but using generic delegates instead
    internal static class SafeEvents {
        internal static void InvokeSafe<T>(this T action, string task) where T : Delegate
        {
            if (action == null)
            {
                return;
            }

            Delegate[] invocationList = action.GetInvocationList();
            foreach (Delegate del in invocationList)
            {
                try
                {
                    del.DynamicInvoke();
                }
                catch (Exception e)
                {
                    FusionLogger.LogException(task, e);
                }
            }
        }

        internal static void InvokeSafe<T, T1>(this T action, T1 param, string task) where T : Delegate
        {
            if (action == null)
            {
                return;
            }

            Delegate[] invocationList = action.GetInvocationList();
            foreach (Delegate del in invocationList)
            {
                try
                {
                    del.DynamicInvoke(param);
                }
                catch (Exception e)
                {
                    FusionLogger.LogException(task, e);
                }
            }
        }

        internal static void InvokeSafe<T, T1, T2>(this T action, T1 param1, T2 param2, string task) where T : Delegate
        {
            if (action == null) {
                return;
            }

            Delegate[] invocationList = action.GetInvocationList();
            foreach (Delegate del in invocationList)
            {
                try
                {
                    del.DynamicInvoke(param1, param2);
                }
                catch (Exception e)
                {
                    FusionLogger.LogException(task, e);
                }
            }
        }

        internal static void InvokeSafe<T, T1, T2, T3>(this T action, T1 param1, T2 param2, T3 param3, string task) where T : Delegate
        {
            if (action == null)
            {
                return;
            }

            Delegate[] invocationList = action.GetInvocationList();
            foreach (Delegate del in invocationList)
            {
                try
                {
                    del.DynamicInvoke(param1, param2, param3);
                }
                catch (Exception e)
                {
                    FusionLogger.LogException(task, e);
                }
            }
        }
    }
}
