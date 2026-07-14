using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;

using LabFusion.MonoBehaviours;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Marrow.Rig;

/// <summary>
/// Manages the creation of stripped dummy RigManagers to represent humanoids.
/// </summary>
public static class DummyRigCreator
{
    public struct DummyRigCreationInfo
    {
        public Vector3 Position;

        public Quaternion Rotation;

        public Action<RigManager> BeforeEnableCallback;

        public Action<RigManager> SpawnCallback;
    }

    /// <summary>
    /// This is the base name of dummy rigs. This is a placeholder for before other rig spawners set their own name.
    /// </summary>
    public const string DummyRigName = "[RigManager (Dummy)]";

    /// <summary>
    /// Creates a stripped dummy RigManager.
    /// </summary>
    /// <param name="spawnCallback"></param>
    public static void CreateDummyRig(DummyRigCreationInfo info)
    {
        var marrowSettings = MarrowSettings.RuntimeInstance;

        if (marrowSettings == null)
        {
            return;
        }

        var defaultPlayerRig = marrowSettings.DefaultPlayerRig.Crate;

        if (defaultPlayerRig == null)
        {
            return;
        }

        defaultPlayerRig.LoadAsset((Action<GameObject>)((go) => OnDefaultRigLoaded(go, info)));
    }

    /// <summary>
    /// Replaces the controllers on a ControllerRig with dummy equivalents.
    /// </summary>
    /// <param name="controllerRig"></param>
    public static void DummifyControllers(ControllerRig controllerRig)
    {
        var originalLeftController = controllerRig.leftController;
        var originalRightController = controllerRig.rightController;

        // Left controller
        var leftHaptor = controllerRig.leftController.haptor;
        controllerRig.leftController = controllerRig.leftController.gameObject.AddComponent<BaseController>();
        controllerRig.leftController.contRig = controllerRig;
        leftHaptor.device_Controller = controllerRig.leftController;
        controllerRig.leftController.handedness = Handedness.LEFT;

        // Right controller
        var rightHaptor = controllerRig.rightController.haptor;
        controllerRig.rightController = controllerRig.rightController.gameObject.AddComponent<BaseController>();
        controllerRig.rightController.contRig = controllerRig;
        rightHaptor.device_Controller = controllerRig.rightController;
        controllerRig.rightController.handedness = Handedness.RIGHT;

        // Destroy the original controllers
        GameObject.DestroyImmediate(originalLeftController);
        GameObject.DestroyImmediate(originalRightController);
    }

    private static void OnDefaultRigLoaded(GameObject asset, DummyRigCreationInfo info)
    {
        var rigManagerAsset = asset.GetComponentInChildren<RigManager>().gameObject;

        var newRigGameObject = GameObject.Instantiate(rigManagerAsset, DisabledContainer.ContainerTransform);
        newRigGameObject.name = DummyRigName;
        newRigGameObject.SetActive(false);

        newRigGameObject.transform.SetPositionAndRotation(info.Position, info.Rotation);

        var newRigManager = newRigGameObject.GetComponent<RigManager>();
        ConvertToDummyRig(newRigManager);

        info.BeforeEnableCallback?.Invoke(newRigManager);

        newRigGameObject.transform.parent = null;
        newRigGameObject.SetActive(true);

        info.SpawnCallback?.Invoke(newRigManager);
    }

    private static void ConvertToDummyRig(RigManager rigManager)
    {
        // Since the dummy rig is not part of the pool, an AntiHasher needs to be added
        // This prevents Fusion from hashing its MarrowEntity, which could cause syncing issues
        rigManager.gameObject.AddComponent<AntiHasher>();

        // Strip all components from the rig that are unnecessary/interfere with the local player
        RigStripper.StripRigManager(rigManager);

        // Replaces the controllers with dummy controllers
        DummifyControllers(rigManager.ControllerRig);
    }
}
