using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.SceneManagement;

using LabFusion.Extensions;

using IL2GoList = Il2CppSystem.Collections.Generic.List<UnityEngine.GameObject>;

namespace LabFusion.Utilities
{
    public static partial class GameObjectUtilities
    {
        internal static async Task<IL2GoList> FindRootsWithNameAsync(string scene, string name)
        {
            await Task.Delay(16);

            Scene sceneAsset = default;
            bool isFinished = false;

            ThreadingUtilities.RunSynchronously(() => {
                sceneAsset = SceneManager.GetSceneByName(scene);
                isFinished = true;
            });

            while (!isFinished)
                await Task.Delay(16);

            ThreadingUtilities.IL2PrepareThread();
            IL2GoList buffer = new();
            isFinished = false;
            bool isValid = sceneAsset.IsValid();

            if (!isValid)
                return buffer;

            ThreadingUtilities.RunSynchronously(() => {
                buffer.Capacity = sceneAsset.rootCount;
                sceneAsset.GetRootGameObjects(buffer);
                isFinished = true;
            });

            while (!isFinished)
                await Task.Delay(16);

            ThreadingUtilities.IL2PrepareThread();

            buffer.RemoveAll((Il2CppSystem.Predicate<GameObject>)(g => g.name != name));

            return buffer;
        }

        internal static async Task<int> GetRootIndexAsync(this GameObject go)
        {
            await Task.Delay(16);

            ThreadingUtilities.IL2PrepareThread();
            string sceneName = go.scene.name;
            string name = go.name;

            var objects = await FindRootsWithNameAsync(sceneName, name);

            ThreadingUtilities.IL2PrepareThread();
            var index = objects.FindIndex((Il2CppSystem.Predicate<GameObject>)(g => g == go));

            return index;
        }

        internal static async Task<GameObject> GetRootByIndexAsync(string scene, int index, string name)
        {
            await Task.Delay(16);

            var matching = await FindRootsWithNameAsync(scene, name);

            ThreadingUtilities.IL2PrepareThread();
            GameObject go = null;
            if (matching == null || matching.Count == 0)
            {
#if DEBUG
                FusionLogger.Warn("Failed to find a list of matching root GameObjects! Searching for root by name!");
#endif

                go = GameObject.Find($"/{name}");
            }
            else if (matching.Count <= index)
                go = matching[matching.Count - 1];
            else
                go = matching[index];

            return go;
        }

        public static async Task<string> GetFullPathAsync(this GameObject go, Action<string> onComplete = null)
        {
            await Task.Delay(16);

            string path = null;
            ThreadingUtilities.IL2PrepareThread();

            try
            {
                Transform transform = go.transform;
                GameObject root = transform.root.gameObject;

                var index = await root.GetRootIndexAsync();

                ThreadingUtilities.IL2PrepareThread();

                path = $"{go.scene.name}{PathSeparator}{index}{transform.GetBasePath()}";
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

            if (onComplete != null)
                ThreadingUtilities.RunSynchronously(() => { onComplete(path); });

            return path;
        }

        public static async Task<GameObject> GetGameObjectAsync(string path, Action<GameObject> onComplete = null)
        {
            await Task.Delay(16);

            string[] parts = path.Split(PathSeparator);
            string scene = parts[0];
            int index = int.Parse(parts[1]);
            string name = parts[2];

            ThreadingUtilities.IL2PrepareThread();

            GameObject result = null;

            try
            {
                var root = await GetRootByIndexAsync(scene, index, name);

                ThreadingUtilities.IL2PrepareThread();

                if (root != null)
                {
                    Transform child = root.transform;
                    for (var i = 3; i < parts.Length; i++)
                    {
                        child = child.GetTransformByIndex(int.Parse(parts[i++]), parts[i]);
                    }

                    result = child.gameObject;

                    if (onComplete != null)
                        ThreadingUtilities.RunSynchronously(() => { onComplete(result); });
                }
            }
            catch (Exception e)
            {
#if DEBUG
                FusionLogger.LogException("finding GameObject by path", e);
#endif
            };

            return result;
        }
    }
}
