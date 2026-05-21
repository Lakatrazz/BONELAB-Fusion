using Il2CppSLZ.Marrow;

using LabFusion.Player;

using UnityEngine;

namespace LabFusion.SDK.Wearables;

public interface IWearableComponent
{
    void OnInitialize(bool local, PlayerID playerID = null);

    void OnMainInstanceCreated(GameObject mainInstance, RigManager rigManager);

    void OnReflectionInstanceCreated(GameObject reflectionInstance);

    void OnTick(float deltaTime);

    void OnDeinitialize();
}
