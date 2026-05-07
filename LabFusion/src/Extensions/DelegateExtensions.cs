using LabFusion.Utilities;

namespace LabFusion.Extensions;

public static class DelegateExtensions
{
    /// <summary>
    /// Invokes a delegate with each call wrapped in a try catch.
    /// </summary>
    /// <typeparam name="T">The delegate type.</typeparam>
    /// <param name="action">The delegate to invoke.</param>
    /// <param name="task">The task to log if an invocation fails.</param>
    public static void InvokeSafe<T>(this T action, string task) where T : Delegate
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

    /// <summary>
    /// Invokes a delegate with a single parameter with each call wrapped in a try catch.
    /// </summary>
    /// <typeparam name="T">The delegate type.</typeparam>
    /// <typeparam name="T1">The parameter type.</typeparam>
    /// <param name="action">The delegate to invoke.</param>
    /// <param name="param">The parameter.</param>
    /// <param name="task">The task to log if an invocation fails.</param>
    public static void InvokeSafe<T, T1>(this T action, T1 param, string task) where T : Delegate
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

    /// <summary>
    /// Invokes a delegate with two parameters with each call wrapped in a try catch.
    /// </summary>
    /// <typeparam name="T">The delegate type.</typeparam>
    /// <typeparam name="T1">The first parameter type.</typeparam>
    /// <typeparam name="T2">The second parameter type.</typeparam>
    /// <param name="action">The delegate to invoke.</param>
    /// <param name="param1">The first parameter.</param>
    /// <param name="param2">The second parameter.</param>
    /// <param name="task">The task to log if an invocation fails.</param>
    public static void InvokeSafe<T, T1, T2>(this T action, T1 param1, T2 param2, string task) where T : Delegate
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
                del.DynamicInvoke(param1, param2);
            }
            catch (Exception e)
            {
                FusionLogger.LogException(task, e);
            }
        }
    }

    /// <summary>
    /// Invokes a delegate with three parameters with each call wrapped in a try catch.
    /// </summary>
    /// <typeparam name="T">The delegate type.</typeparam>
    /// <typeparam name="T1">The first parameter type.</typeparam>
    /// <typeparam name="T2">The second parameter type.</typeparam>
    /// <typeparam name="T3">The third parameter type.</typeparam>
    /// <param name="action">The delegate to invoke.</param>
    /// <param name="param1">The first parameter.</param>
    /// <param name="param2">The second parameter.</param>
    /// <param name="param3">The third parameter.</param>
    /// <param name="task">The task to log if an invocation fails.</param>
    public static void InvokeSafe<T, T1, T2, T3>(this T action, T1 param1, T2 param2, T3 param3, string task) where T : Delegate
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
