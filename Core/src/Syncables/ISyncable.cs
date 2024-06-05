using System;
using LabFusion.Utilities;
using SLZ.Interaction;

namespace LabFusion.Syncables
{
    public interface ISyncable
    {
        void InsertCatchupDelegate(Action<ulong> catchup);

        void ClearCatchupDelegate();

        void InvokeCatchup(ulong user);

        Grip GetGrip(ushort index);

        bool IsGrabbed();

        void SetOwner(byte owner);

        byte? GetOwner();

        void RemoveOwner();

        bool IsOwner();

        bool IsQueued();

        bool IsRegistered();

        void HookOnRegistered(Action action);

        void Cleanup();

        void OnRegister(ushort id);

        ushort? GetIndex(Grip grip);

        ushort GetId();

        void OnPreFixedUpdate();
        void OnFixedUpdate();

        void OnParallelFixedUpdate();

        void OnUpdate();

        void OnParallelUpdate();

        bool IsDestroyed();
    }

    public abstract class Syncable : ISyncable
    {
        public ushort Id;

        private bool _hasRegistered = false;

        private bool _wasDisposed = false;

        private Action _onRegistered = null;

        private Action<ulong> _catchupDelegate;

        public abstract bool IsGrabbed();
        public abstract Grip GetGrip(ushort index);
        public abstract ushort? GetIndex(Grip grip);

        public void InsertCatchupDelegate(Action<ulong> catchup)
        {
            _catchupDelegate += catchup;
        }

        public void ClearCatchupDelegate()
        {
            _catchupDelegate = null;
        }

        public void InvokeCatchup(ulong user)
        {
            // Send any stored catchup info for our object
            _catchupDelegate?.InvokeSafe(user, "executing Catchup Delegate");
        }

        public abstract byte? GetOwner();
        public abstract bool IsOwner();
        public abstract void RemoveOwner();
        public abstract void SetOwner(byte owner);

        public abstract void OnFixedUpdate();
        public abstract void OnUpdate();

        public virtual void OnPreFixedUpdate() { }

        public virtual void OnParallelFixedUpdate() { }
        public virtual void OnParallelUpdate() { }

        public virtual void Cleanup()
        {
            _wasDisposed = true;

            ClearCatchupDelegate();
        }

        public virtual void OnRegister(ushort id)
        {
            Id = id;
            _hasRegistered = true;

            _onRegistered?.Invoke();
            _onRegistered = null;
        }

        public void HookOnRegistered(Action action)
        {
            if (!IsRegistered())
            {
                _onRegistered += action;
            }
            else
            {
                action();
            }
        }

        public bool IsQueued() => !IsRegistered() && !IsDestroyed();
        public bool IsRegistered() => _hasRegistered;
        public bool IsDestroyed() => _wasDisposed;

        public ushort GetId()
        {
            return Id;
        }
    }
}
