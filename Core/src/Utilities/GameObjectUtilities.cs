using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

using LabFusion.Extensions;

namespace LabFusion.Utilities {
    public static class GameObjectUtilities {
        public const char PathSeparator = '¬';

        private static GameObject[] _rootObjectBuffer;

        internal static List<GameObject> FindRootsWithName(string scene, string name) {
            var gameObjects = new List<GameObject>();

            var sceneAsset = SceneManager.GetSceneByName(scene);
            if (!sceneAsset.IsValid())
                return gameObjects;

            _rootObjectBuffer = sceneAsset.GetRootGameObjects();

            for (var i = 0; i < sceneAsset.rootCount; i++) {
                var go = _rootObjectBuffer[i];
                if (go != null && go.name == name)
                    gameObjects.Add(go);
            }

            _rootObjectBuffer = null;

            return gameObjects;
        }

        internal static int GetRootIndex(this GameObject go)
        {
            var objects = FindRootsWithName(go.scene.name, go.name);
            for (var i = 0; i < objects.Count; i++)
            {
                if (objects[i] == go)
                    return i;
            }
            return -1;
        }

        internal static GameObject GetRootByIndex(string scene, int index, string name)
        {
            var matching = FindRootsWithName(scene, name);

            if (matching.Count == 0) {
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

        public static string GetFullPath(this GameObject go) {
            try {
                return $"{go.scene.name}{PathSeparator}{go.transform.root.gameObject.GetRootIndex()}{go.transform.GetBasePath()}";
            }
            catch { }

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
