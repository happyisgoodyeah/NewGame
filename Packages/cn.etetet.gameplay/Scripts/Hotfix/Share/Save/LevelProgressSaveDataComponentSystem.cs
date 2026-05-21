using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 关卡进度存档数据维护逻辑
    /// </summary>
    [EntitySystemOf(typeof(LevelProgressSaveDataComponent))]
    public static partial class LevelProgressSaveDataComponentSystem
    {
        /// <summary>
        /// 初始化关卡进度存档数据
        /// </summary>
        [EntitySystem]
        private static void Awake(this LevelProgressSaveDataComponent self)
        {
            self.DataVersion = SaveConst.CurrentDataVersion;
            self.CurrentLevelConfigId = 0;
            self.UnlockedLevelIds = new List<int>();
            self.PassedLevelIds = new List<int>();
            self.LastModifiedTime = DateTime.Now;
            self.EnsureDefaultUnlockedLevel();
        }

        /// <summary>
        /// 补齐反序列化后可能缺失的集合和默认解锁关卡
        /// </summary>
        /// <param name="self">关卡进度存档数据</param>
        public static void EnsureRuntimeState(this LevelProgressSaveDataComponent self)
        {
            self.UnlockedLevelIds ??= new List<int>();
            self.PassedLevelIds ??= new List<int>();
            if (self.DataVersion <= 0)
            {
                self.DataVersion = SaveConst.CurrentDataVersion;
            }

            self.EnsureDefaultUnlockedLevel();
        }

        /// <summary>
        /// 判断关卡是否已经解锁
        /// </summary>
        /// <param name="self">关卡进度存档数据</param>
        /// <param name="gridConfigId">Grid 配置 id</param>
        /// <returns>是否已解锁</returns>
        public static bool IsLevelUnlocked(this LevelProgressSaveDataComponent self, int gridConfigId)
        {
            self.EnsureRuntimeState();
            return self.UnlockedLevelIds.Contains(gridConfigId);
        }

        /// <summary>
        /// 判断关卡是否已经通关
        /// </summary>
        /// <param name="self">关卡进度存档数据</param>
        /// <param name="gridConfigId">Grid 配置 id</param>
        /// <returns>是否已通关</returns>
        public static bool IsLevelPassed(this LevelProgressSaveDataComponent self, int gridConfigId)
        {
            self.EnsureRuntimeState();
            return self.PassedLevelIds.Contains(gridConfigId);
        }

        /// <summary>
        /// 记录最近进入的关卡
        /// </summary>
        /// <param name="self">关卡进度存档数据</param>
        /// <param name="gridConfigId">Grid 配置 id</param>
        public static void SetCurrentLevel(this LevelProgressSaveDataComponent self, int gridConfigId)
        {
            self.EnsureRuntimeState();
            self.CurrentLevelConfigId = gridConfigId;
            self.LastModifiedTime = DateTime.Now;
        }

        /// <summary>
        /// 标记关卡已通关并解锁下一关
        /// </summary>
        /// <param name="self">关卡进度存档数据</param>
        /// <param name="gridConfigId">Grid 配置 id</param>
        public static void PassLevel(this LevelProgressSaveDataComponent self, int gridConfigId)
        {
            self.EnsureRuntimeState();
            if (!self.PassedLevelIds.Contains(gridConfigId))
            {
                self.PassedLevelIds.Add(gridConfigId);
            }

            self.UnlockLevel(gridConfigId);
            int nextLevelId = GetNextLevelConfigId(gridConfigId);
            if (nextLevelId > 0)
            {
                self.UnlockLevel(nextLevelId);
            }

            self.LastModifiedTime = DateTime.Now;
        }

        /// <summary>
        /// 解锁指定关卡
        /// </summary>
        /// <param name="self">关卡进度存档数据</param>
        /// <param name="gridConfigId">Grid 配置 id</param>
        private static void UnlockLevel(this LevelProgressSaveDataComponent self, int gridConfigId)
        {
            if (gridConfigId > 0 && !self.UnlockedLevelIds.Contains(gridConfigId))
            {
                self.UnlockedLevelIds.Add(gridConfigId);
            }
        }

        /// <summary>
        /// 确保首个配置关卡默认解锁
        /// </summary>
        /// <param name="self">关卡进度存档数据</param>
        private static void EnsureDefaultUnlockedLevel(this LevelProgressSaveDataComponent self)
        {
            int firstLevelId = GetFirstLevelConfigId();
            if (firstLevelId > 0)
            {
                self.UnlockLevel(firstLevelId);
            }
        }

        /// <summary>
        /// 获取第一个关卡配置 id
        /// </summary>
        /// <returns>第一个关卡配置 id</returns>
        private static int GetFirstLevelConfigId()
        {
            List<GridConfig> dataList = GridConfigCategory.Instance.DataList;
            return dataList is { Count: > 0 } ? dataList[0].Id : 0;
        }

        /// <summary>
        /// 获取当前关卡之后的下一个配置 id
        /// </summary>
        /// <param name="gridConfigId">当前 Grid 配置 id</param>
        /// <returns>下一关配置 id</returns>
        private static int GetNextLevelConfigId(int gridConfigId)
        {
            List<GridConfig> dataList = GridConfigCategory.Instance.DataList;
            for (int i = 0; i < dataList.Count; ++i)
            {
                if (dataList[i].Id == gridConfigId)
                {
                    return i + 1 < dataList.Count ? dataList[i + 1].Id : 0;
                }
            }

            return 0;
        }
    }
}
