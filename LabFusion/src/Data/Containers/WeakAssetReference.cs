using Object = UnityEngine.Object;

namespace LabFusion.Data
{
    public class WeakAssetReference<T> where T : Object
    {
        private T _asset;
        private bool _hasAsset = false;
        private Action<T> _onLoaded = null;

        public bool HasAsset => _hasAsset;
        public T Asset => _asset;

        public void Load(Action<T> onLoaded)
        {
            if (_hasAsset)
            {
                onLoaded(_asset);
            }
            else
            {
                _onLoaded += onLoaded;
            }
        }

        public void SetAsset(T asset)
        {
            if (_hasAsset)
                return;

            _asset = asset;
            _hasAsset = true;

            _onLoaded?.Invoke(asset);
        }

        public void UnloadAsset()
        {
            if (!_hasAsset)
                return;

            _asset = null;
            _hasAsset = false;
        }

        public static implicit operator T(WeakAssetReference<T> reference)
        {
            return reference.Asset;
        }
    }
}