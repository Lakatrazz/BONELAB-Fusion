using Il2CppSLZ.Marrow.Pool;

using LabFusion.Marrow;
using LabFusion.Marrow.Pool;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Entities;

public class NetworkPropGhost : IEntityExtender, IEntityPosableExtender, IEntityDespawnableExtender, IEntityFixedUpdatable, IProgress<float>
{
    private static readonly int FillID = Shader.PropertyToID("_Fill");

    public bool IsRegistered { get; private set; } = false;

    public NetworkEntity NetworkEntity { get; private set; } = null;

    public Bounds Bounds { get; private set; } = default;

    public Poolee GhostPoolee { get; private set; } = null;

    public Transform GhostRoot { get; private set; } = null;

    public Transform GhostOrigin { get; private set; } = null;

    public GameObject DownloadingRoot { get; private set; } = null;
    public GameObject FailedRoot { get; private set; } = null;

    public MeshRenderer DownloadingInsideRenderer { get; private set; } = null;
    public MeshRenderer FailedInsideRenderer { get; private set; } = null;

    public bool HasDownloadFailed { get; private set; } = false;

    public float DownloadProgress { get; private set; } = 0f;

    public EntityPoseReceiver PoseReceiver { get; private set; } = new();

    public NetworkPropGhost(NetworkEntity networkEntity, Bounds bounds)
    {
        NetworkEntity = networkEntity;
        Bounds = bounds;

        networkEntity.ConnectExtender(this);
    }

    public void OnPoseReceived(EntityPose pose)
    {
        if (PoseReceiver.BodyCount != pose.BodyCount)
        {
            PoseReceiver.InitializePoses(pose.BodyCount);
        }

        PoseReceiver.ReceivePose(pose);
    }

    public void OnExtenderRegistered()
    {
        IsRegistered = true;

        CreateGhost();

        NetworkEntityManager.UpdatableManager.FixedUpdateManager.Register(this);
    }

    public void OnExtenderUnregistered()
    {
        IsRegistered = false;

        DestroyGhost();

        NetworkEntityManager.UpdatableManager.FixedUpdateManager.Unregister(this);
    }

    private void CreateGhost()
    {
        var ghostSpawnable = LocalAssetSpawner.CreateSpawnable(FusionSpawnableReferences.EntityGhostReference);
        LocalAssetSpawner.Register(ghostSpawnable);

        LocalAssetSpawner.Spawn(ghostSpawnable, Vector3.zero, Quaternion.identity, OnGhostSpawned);
    }

    private void DestroyGhost()
    {
        if (GhostPoolee != null)
        {
            GhostPoolee.Despawn();
        }
    }
    
    private void OnGhostSpawned(Poolee poolee)
    {
        if (!IsRegistered)
        {
            poolee.Despawn();
            return;
        }

        GhostPoolee = poolee;
        GhostRoot = poolee.transform;

        GhostOrigin = GhostRoot.Find("Origin");

        if (GhostOrigin == null)
        {
            return;
        }

        GhostOrigin.localPosition = Bounds.center;
        GhostOrigin.localScale = Bounds.size;

        var downloadingRoot = GhostOrigin.Find("Downloading");
        DownloadingRoot = downloadingRoot.gameObject;

        var downloadingInside = downloadingRoot.Find("Inside");

        if (downloadingInside != null)
        {
            DownloadingInsideRenderer = downloadingInside.GetComponent<MeshRenderer>();
        }

        var failedRoot = GhostOrigin.Find("Failed");
        FailedRoot = failedRoot.gameObject;

        var failedInside = failedRoot.Find("Inside");

        if (failedInside != null)
        {
            FailedInsideRenderer = failedInside.GetComponent<MeshRenderer>();
        }

        ApplyDownloadResult();
        ApplyDownloadProgress();
    }

    public void Report(float value)
    {
        DownloadProgress = value;

        ApplyDownloadProgress();
    }

    public void OnDownloadFailed()
    {
        HasDownloadFailed = true;

        ApplyDownloadResult();
    }

    private void ApplyDownloadResult()
    {
        if (DownloadingRoot != null)
        {
            DownloadingRoot.SetActive(!HasDownloadFailed);
        }

        if (FailedRoot != null)
        {
            FailedRoot.SetActive(HasDownloadFailed);
        }
    }

    private void ApplyDownloadProgress()
    {
        MaterialPropertyBlock propertyBlock = new();
        propertyBlock.SetFloat(FillID, DownloadProgress);

        if (DownloadingInsideRenderer != null)
        {
            DownloadingInsideRenderer.SetPropertyBlock(propertyBlock);
        }

        if (FailedInsideRenderer != null)
        {
            FailedInsideRenderer.SetPropertyBlock(propertyBlock);
        }
    }

    public void OnDespawnReceived() { }

    public void PlayDespawnEffect() { }

    public void OnEntityFixedUpdate(float deltaTime)
    {
        if (!PoseReceiver.HasReceivedPose)
        {
            return;
        }

        float unscaledDeltaTime = deltaTime / TimeReferences.SafeTimeScale;

        PoseReceiver.TickPose(unscaledDeltaTime);

        if (GhostRoot != null)
        {
            var bodyPose = PoseReceiver.InterpolatedPose.Bodies[0];

            GhostRoot.SetPositionAndRotation(bodyPose.Position, bodyPose.Rotation);
        }
    }
}
