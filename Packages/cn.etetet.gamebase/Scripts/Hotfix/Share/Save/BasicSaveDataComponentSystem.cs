using System;

namespace ET
{
    [EntitySystemOf(typeof(BasicSaveDataComponent))]
    public static partial class BasicSaveDataComponentSystem
    {
        /// <summary>
        /// 初始化基础存档数据默认值
        /// </summary>
        /// <param name="self">基础存档数据组件</param>
        [EntitySystem]
        private static void Awake(this BasicSaveDataComponent self)
        {
            self.DataVersion = SaveConst.CurrentDataVersion;
            self.LastModifiedTime = DateTime.Now;
        }

        /// <summary>
        /// 设置基础玩家信息
        /// </summary>
        /// <param name="self">基础存档数据组件</param>
        /// <param name="playerId">玩家标识</param>
        /// <param name="playerName">玩家名称</param>
        public static void SetPlayer(this BasicSaveDataComponent self, string playerId, string playerName)
        {
            self.PlayerId = playerId ?? string.Empty;
            self.PlayerName = string.IsNullOrWhiteSpace(playerName) ? "New Player" : playerName;
            self.LastModifiedTime = DateTime.Now;
        }

        /// <summary>
        /// 设置简单进度数据
        /// </summary>
        /// <param name="self">基础存档数据组件</param>
        /// <param name="sceneName">场景名</param>
        /// <param name="progress">进度值</param>
        public static void SetProgress(this BasicSaveDataComponent self, string sceneName, int progress)
        {
            self.SceneName = sceneName ?? string.Empty;
            self.Progress = progress;
            self.LastModifiedTime = DateTime.Now;
        }
    }
}
