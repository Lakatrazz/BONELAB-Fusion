using UnityEngine;

namespace LabFusion.Entities;

public class NetworkPropGhost : IEntityExtender, IEntityPosableExtender
{
    public NetworkEntity NetworkEntity { get; private set; } = null;

    public Transform TestTransform;

    public NetworkPropGhost(NetworkEntity networkEntity)
    {
        NetworkEntity = networkEntity;

        networkEntity.ConnectExtender(this);
    }

    public void ReceivePose(EntityPose entityPose)
    {
        if (TestTransform != null)
        {
            var pose = entityPose.Bodies[0];
            TestTransform.SetPositionAndRotation(pose.Position, pose.Rotation);
        }
    }

    public void OnExtenderRegistered()
    {
        CreateGhost();
    }

    public void OnExtenderUnregistered()
    {
        DestroyGhost();
    }

    private void CreateGhost()
    {
        var testCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject.Destroy(testCube.GetComponent<BoxCollider>());
        testCube.GetComponent<MeshRenderer>().sharedMaterial = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(m => m.name.ToLower().Contains("concrete"));
        TestTransform = testCube.transform;
    }

    private void DestroyGhost()
    {
        if (TestTransform != null)
        {
            GameObject.Destroy(TestTransform.gameObject);
        }
    }
}
