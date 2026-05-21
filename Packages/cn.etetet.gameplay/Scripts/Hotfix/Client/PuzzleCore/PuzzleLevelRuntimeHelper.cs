using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 关卡运行时场景和数据切换辅助方法
    /// </summary>
    public static class PuzzleLevelRuntimeHelper
    {
        private const string PuzzleSceneName = "Test";

        /// <summary>
        /// 确保当前客户端已经进入 Puzzle 使用的 Unity 场景
        /// </summary>
        /// <param name="root">客户端根场景</param>
        /// <returns>当前关卡逻辑场景</returns>
        public static async ETTask<Scene> EnsurePuzzleSceneAsync(Scene root)
        {
            Scene currentScene = root.CurrentScene();
            if (currentScene != null && !currentScene.IsDisposed && currentScene.Name == PuzzleSceneName)
            {
                return currentScene;
            }

            await EnterMapHelper.EnterMapAsync(root);
            return root.CurrentScene();
        }

        /// <summary>
        /// 清理当前场景中已有的关卡数据
        /// </summary>
        /// <param name="scene">当前关卡逻辑场景</param>
        public static void ClearCurrentLevel(Scene scene)
        {
            if (scene == null || scene.IsDisposed || scene.ChildrenCount() <= 0)
            {
                return;
            }

            List<Entity> needDisposeEntities = new List<Entity>();
            foreach (Entity child in scene.Children.Values)
            {
                if (child is Grid or Puzzle)
                {
                    needDisposeEntities.Add(child);
                }
            }

            foreach (Entity entity in needDisposeEntities)
            {
                entity.Dispose();
            }
        }

        /// <summary>
        /// 清理旧关卡并创建新的关卡数据
        /// </summary>
        /// <param name="scene">当前关卡逻辑场景</param>
        /// <param name="gridConfigId">Grid 配置 id</param>
        /// <returns>创建完成的 Grid</returns>
        public static Grid StartLevel(Scene scene, int gridConfigId)
        {
            ClearCurrentLevel(scene);
            return PuzzleBootstrapHelper.CreateGrid(scene, gridConfigId);
        }
    }
}
