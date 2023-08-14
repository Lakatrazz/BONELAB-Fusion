using LabFusion.Data;

using MelonLoader;

using SLZ.Marrow.Warehouse;

using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Avatar = SLZ.VRMK.Avatar;

namespace LabFusion.Extensions {
    public static class RigManagerExtensions {
        public static void SwapAvatarCrate(this RigReferenceCollection references, string barcode, Action<bool> callback = null, Action<string, GameObject> preSwapAvatar = null) {
            AvatarCrateReference crateRef = new(barcode);
            var crate = crateRef.Crate;

            if (crate == null) {
                callback?.Invoke(false);
            }
            else {
                MelonCoroutines.Start(CoWaitAndSwapAvatarRoutine(references, crate, callback, preSwapAvatar));
            }
        }

        private static IEnumerator CoWaitAndSwapAvatarRoutine(RigReferenceCollection references, AvatarCrate crate, Action<bool> callback = null, Action<string, GameObject> preSwapAvatar = null)
        {
            bool loaded = false;
            GameObject avatar = null;

            crate.LoadAsset((Il2CppSystem.Action<GameObject>)((go) =>
            {
                loaded = true;
                avatar = go;
            }));

            while (!loaded)
                yield return null;

            if (!references.IsValid)
                yield break;

            if (avatar == null)
            {
                callback?.Invoke(false);
            }
            else
            {
                var rm = references.RigManager;
                GameObject instance = GameObject.Instantiate(avatar);
                instance.SetActive(false);
                instance.name = avatar.name;

                preSwapAvatar?.Invoke(crate.Barcode, instance);

                instance.transform.parent = references.RigManager.transform;
                instance.transform.localPosition = Vector3Extensions.zero;
                instance.transform.localRotation = QuaternionExtensions.identity;

                var avatarComponent = instance.GetComponentInParent<Avatar>(true);
                rm.SwapAvatar(avatarComponent);
                
                while (references.IsValid && rm.avatar != avatarComponent)
                    yield return null;

                if (!references.IsValid)
                    yield break;

                rm._avatarCrate = new AvatarCrateReference(crate.Barcode);
                rm.onAvatarSwapped?.Invoke();
                rm.onAvatarSwapped2?.Invoke(crate.Barcode);
                callback?.Invoke(true);
            }
        }

    }
}
