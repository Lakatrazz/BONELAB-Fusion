using HarmonyLib;

using LabFusion.Extensions;
using LabFusion.Math;
using LabFusion.Network;
using LabFusion.Utilities;

using System.Reflection;

namespace LabFusion.RPC;

using Harmony = HarmonyLib.Harmony;

public static class RpcManager
{
    public static Harmony HarmonyInstance { get; private set; }

    public static Dictionary<long, MethodBase> HashToMethod { get; private set; } = new();
    public static Dictionary<MethodBase, long> MethodToHash { get; private set; } = new();
    public static Dictionary<MethodBase, RpcAttribute> MethodToRpc { get; private set; } = new();

    public static bool InvokingRpc { get; set; } = false;

    public static void OnInitialize()
    {
        HarmonyInstance = new Harmony("com.fusion.rpc");
    }

    public static void LoadRpcs(Assembly assembly)
    {
        if (!assembly.IsValid())
        {
            FusionLogger.Error($"Assembly {assembly.FullName} is invalid and rpcs will not be loaded.");
            return;
        }

        var types = assembly.GetTypes();

        foreach (var type in types)
        {
            if (!type.IsValid())
            {
                continue;
            }

            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                LoadRpc(method);
            }
        }
    }

    public static void LoadRpc(MethodInfo method)
    {
        var rpcAttribute = method.GetCustomAttribute<RpcAttribute>();

        if (rpcAttribute == null)
        {
            return;
        }

        string typeName = method.DeclaringType.AssemblyQualifiedName;
        string methodName = method.GetNameWithParameters();

        long hash = BitMath.MakeLong(typeName.GetDeterministicHashCode(), methodName.GetDeterministicHashCode());

        rpcAttribute.SetHash(hash);

        HashToMethod[hash] = method;
        MethodToHash[method] = hash;
        MethodToRpc[method] = rpcAttribute;

        HarmonyInstance.Patch(method, new(typeof(RpcManager).GetMethod(nameof(StaticRpcPrefix), BindingFlags.NonPublic | BindingFlags.Static)));
    }

    private static bool StaticRpcPrefix(object[] __args, MethodBase __originalMethod)
    {
        if (InvokingRpc)
        {
            InvokingRpc = false;
            return true;
        }

        if (!NetworkInfo.HasServer)
        {
            return false;
        }

        if (!MethodToRpc.TryGetValue(__originalMethod, out var rpc))
        {
            return false;
        }

        var data = new RPCMethodData()
        {
            MethodHash = rpc.MethodHash,
            Parameters = __args,
        };

        MessageRelay.RelayNative(data, NativeMessageTag.RPCMethod, new MessageRoute(rpc.RelayType, rpc.Channel));

        return false;
    }
    
    public static void InvokeMethod(RPCMethodData data)
    {
        if (!HashToMethod.TryGetValue(data.MethodHash, out var method))
        {
            return;
        }

        InvokingRpc = true;

        try
        {
            method.Invoke(null, data.Parameters);
        }
        catch (Exception e)
        {
            FusionLogger.LogException($"invoking Rpc {data.MethodHash} named {method.Name}", e);
        }
    }
}
