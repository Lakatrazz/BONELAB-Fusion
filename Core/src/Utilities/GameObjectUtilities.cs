using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

using LabFusion.Extensions;

using IL2GoList = Il2CppSystem.Collections.Generic.List<UnityEngine.GameObject>;

namespace LabFusion.Utilities {
    public static partial class GameObjectUtilities {
        public const char PathSeparator = '¬';

        private static readonly IL2GoList _rootObjectBuffer = new();

        internal static IL2GoList FindRootsWithName(string scene, string name) {
            var sceneAsset = SceneManager.GetSceneByName(scene);
            if (!sceneAsset.IsValid())
                return null;

            _rootObjectBuffer.Capacity = sceneAsset.rootCount;
            sceneAsset.GetRootGameObjects(_rootObjectBuffer);

            _rootObjectBuffer.RemoveAll((Il2CppSystem.Predicate<GameObject>)(g => g.name != name));

            return _rootObjectBuffer;
        }

        internal static int GetRootIndex(this GameObject go)
        {
            var objects = FindRootsWithName(go.scene.name, go.name);
            return objects.FindIndex((Il2CppSystem.Predicate<GameObject>)(g => g == go));
        }

        internal static GameObject GetRootByIndex(string scene, int index, string name)
        {
            var matching = FindRootsWithName(scene, name);

            if (matching == null || matching.Count == 0) {
#if DEBUG
                FusionLogger.Warn("Failed to find a list of matching root GameObjects! Searching for root by name!");
#endif

                return GameObject.Find($"/{name}");
            }
            else if (matching.Count <= index)
                return matching[matching.Count - 1];
            else
                return matching[index];
        }

        public static string GetFullPath(this GameObject go)
        {
            try {
                return $"{go.scene.name}{PathSeparator}{go.transform.root.gameObject.GetRootIndex()}{go.transform.GetBasePath()}";
            }
            catch
#if DEBUG
            (Exception e)
#endif
            {
#if DEBUG
                FusionLogger.LogException("getting path of GameObject", e);
#endif
            }

            return "INVALID_PATH";
        }

        public static GameObject GetGameObject(string path) {
            string[] parts = path.Split(PathSeparator);
            string scene = parts[0];
            int index = int.Parse(parts[1]);
            string name = parts[2];

            try {
                var go = GetRootByIndex(scene, index, name);

                if (go != null) {
                    Transform child = go.transform;
                    for (var i = 3; i < parts.Length; i++) {
                        child = child.GetTransformByIndex(int.Parse(parts[i++]), parts[i]);
                    }
                
                    return child.gameObject;
                }
            }
            catch (Exception e) {
#if DEBUG
                FusionLogger.LogException("finding GameObject by path", e);
#endif
            };

            return null;
        }
    }
}
