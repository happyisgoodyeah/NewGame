using UnityEngine;
using UnityScene = UnityEngine.SceneManagement.Scene;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace ET.Client
{
    /// <summary>
    /// PuzzleSceneRoot 的初始化和根节点查找逻辑。
    /// </summary>
    [EntitySystemOf(typeof(PuzzleSceneRoot))]
    public static partial class PuzzleSceneRootSystem
    {
        private const string SceneRootName = "SceneRoot";
        private const string GridRootName = "GridRoot";

        /// <summary>
        /// 初始化缓存状态。
        /// </summary>
        [EntitySystem]
        private static void Awake(this PuzzleSceneRoot self)
        {
            self.SceneRoot = null;
            self.GridRoot = null;
            self.IsInitialized = false;
        }

        /// <summary>
        /// 释放缓存引用。
        /// </summary>
        [EntitySystem]
        private static void Destroy(this PuzzleSceneRoot self)
        {
            self.SceneRoot = null;
            self.GridRoot = null;
            self.IsInitialized = false;
        }

        /// <summary>
        /// 优先按 ET Scene 名称解析对应的 Unity 场景，找不到时再回退到当前激活场景。
        /// </summary>
        public static bool TryGetUnityScene(this Scene etScene, out UnityScene unityScene)
        {
            unityScene = default;

            UnityScene namedUnityScene = UnitySceneManager.GetSceneByName(etScene.Name);
            if (namedUnityScene.IsValid() && namedUnityScene.isLoaded)
            {
                unityScene = namedUnityScene;
                return true;
            }

            UnityScene activeUnityScene = UnitySceneManager.GetActiveScene();
            if (activeUnityScene.IsValid() && activeUnityScene.isLoaded && activeUnityScene.name == etScene.Name)
            {
                unityScene = activeUnityScene;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 确保当前 ET Scene 已经缓存了当前 Unity 场景中的 SceneRoot 和 GridRoot。
        /// </summary>
        public static PuzzleSceneRoot EnsureInitialized(this Scene etScene)
        {
            PuzzleSceneRoot puzzleSceneRoot = etScene.GetComponent<PuzzleSceneRoot>();
            if (puzzleSceneRoot == null)
            {
                puzzleSceneRoot = etScene.AddComponent<PuzzleSceneRoot>();
            }

            if (puzzleSceneRoot.IsInitialized)
            {
                return puzzleSceneRoot;
            }

            if (!etScene.TryGetUnityScene(out UnityScene unityScene))
            {
                UnityScene activeUnityScene = UnitySceneManager.GetActiveScene();
                throw new UnityException($"can not resolve unity scene for et scene: etScene={etScene.Name} activeUnityScene={activeUnityScene.name}");
            }

            Transform sceneRoot = FindSceneRootTransform(unityScene);
            if (sceneRoot == null)
            {
                throw new UnityException($"can not find {SceneRootName} in unity scene: {unityScene.name}");
            }

            Transform gridRoot = sceneRoot.Find(GridRootName);
            if (gridRoot == null)
            {
                throw new UnityException($"can not find {GridRootName} under {SceneRootName} in unity scene: {unityScene.name}");
            }

            puzzleSceneRoot.SceneRoot = sceneRoot;
            puzzleSceneRoot.GridRoot = gridRoot;
            puzzleSceneRoot.IsInitialized = true;
            return puzzleSceneRoot;
        }

        /// <summary>
        /// 在目标 Unity 场景的根节点中查找 PuzzleCore 使用的 SceneRoot。
        /// </summary>
        private static Transform FindSceneRootTransform(UnityScene unityScene)
        {
            foreach (GameObject rootGameObject in unityScene.GetRootGameObjects())
            {
                if (rootGameObject.name == SceneRootName)
                {
                    return rootGameObject.transform;
                }
            }

            return null;
        }
    }
}
