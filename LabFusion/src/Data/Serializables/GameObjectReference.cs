using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Utilities;
using LabFusion.Marrow.Extenders;

using UnityEngine;

namespace LabFusion.Data.Serializables;

/// <summary>
/// A serializable reference to a networked MarrowBody, or a general GameObject in the scene.
/// <para>This is recommended only as a last resort when there are no optimal solutions.</para>
/// </summary>
public class GameObjectReference : INetSerializable
{
    public enum ReferenceType : byte
    {
        Null,
        Entity,
        Scene
    }

    public ReferenceType Type;

    public GameObject GameObject;

    public NetworkEntityReference? Entity
    {
        get
        {
            if (Type != ReferenceType.Entity)
            {
                return null;
            }

            return _entity;
        }
    }

    public MarrowBody Body => _body;

    private string _path;

    private NetworkEntityReference _entity;
    private ushort _bodyIndex;

    private MarrowBody _body = null;

    private Action<GameObject> _callback = null;

    public GameObjectReference() { }

    public GameObjectReference(GameObject gameObject)
    {
        if (gameObject == null)
        {
            Type = ReferenceType.Null;
            GameObject = null;
            return;
        }

        GameObject = gameObject;

        var marrowBody = MarrowBody.Cache.Get(gameObject);

        if (marrowBody != null && MarrowBodyExtender.Cache.TryGet(marrowBody, out var networkEntity))
        {
            var extender = networkEntity.GetExtender<MarrowBodyExtender>();

            Type = ReferenceType.Entity;
            _entity = new(networkEntity);
            _bodyIndex = extender.GetIndex(marrowBody).Value;
        }
        else
        {
            Type = ReferenceType.Scene;
            _path = GameObjectUtilities.GetFullPath(gameObject);
        }
    }

    public int? GetSize()
    {
        int size = sizeof(byte);

        switch (Type)
        {
            case ReferenceType.Entity:
                size += NetworkEntityReference.Size;
                size += sizeof(ushort);
                break;
            case ReferenceType.Scene:
                size += _path.GetSize();
                break;
        }

        return size;
    }

    public void HookGameObjectFound(Action<GameObject> callback)
    {
        if (GameObject != null)
        {
            callback(GameObject);
        }
        else
        {
            _callback += callback;
        }
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Type);

        switch (Type)
        {
            case ReferenceType.Null:
                if (serializer.IsReader)
                {
                    OnGameObjectFound(null);
                }
                break;
            case ReferenceType.Entity:
                serializer.SerializeValue(ref _entity);
                serializer.SerializeValue(ref _bodyIndex);

                _entity.HookEntityRegistered(OnEntityRegistered);
                break;
            case ReferenceType.Scene:
                serializer.SerializeValue(ref _path);

                if (serializer.IsReader)
                {
                    OnGameObjectFound(GameObjectUtilities.GetGameObject(_path));
                }
                break;
        }
    }

    private void OnEntityRegistered(NetworkEntity entity)
    {
        var extender = entity.GetExtender<MarrowBodyExtender>();

        if (extender == null)
        {
            OnGameObjectFound(null);
            return;
        }

        var marrowBody = extender.GetComponent(_bodyIndex);

        if (marrowBody == null)
        {
            OnGameObjectFound(null);
            return;
        }

        OnGameObjectFound(marrowBody.gameObject);
    }

    private void OnGameObjectFound(GameObject gameObject)
    {
        GameObject = gameObject;

        if (gameObject != null)
        {
            _body = MarrowBody.Cache.Get(gameObject);
        }

        _callback?.Invoke(gameObject);
        _callback = null;
    }
}
