using System;

namespace ET
{
    /// <summary>
    /// Gameplay 存档访问辅助方法
    /// </summary>
    public static class GameplaySaveHelper
    {
        private const string DefaultSlotId = "default";
        private const string DefaultDisplayName = "Default";

        /// <summary>
        /// 确保存在并加载 Gameplay 默认存档
        /// </summary>
        /// <param name="saveManager">存档管理器</param>
        /// <returns>已加载的完整存档数据根</returns>
        public static async ETTask<SaveDataRoot> EnsureGameplaySaveAsync(SaveManagerComponent saveManager)
        {
            if (saveManager == null || !saveManager.IsInitialized)
            {
                return null;
            }

            SaveDataRoot saveDataRoot = await LoadCurrentOrDefaultAsync(saveManager);
            if (saveDataRoot == null)
            {
                SaveSlot saveSlot = await saveManager.CreateSaveAsync(DefaultSlotId, DefaultDisplayName, SaveSlotType.Auto);
                saveDataRoot = saveSlot?.GetDataRoot();
            }

            if (saveDataRoot == null)
            {
                return null;
            }

            LevelProgressSaveDataComponent progressData = EnsureLevelProgressData(saveDataRoot);
            progressData.EnsureRuntimeState();
            await saveManager.SaveCurrentAsync();
            return saveDataRoot;
        }

        /// <summary>
        /// 获取当前已加载存档中的关卡进度
        /// </summary>
        /// <param name="saveManager">存档管理器</param>
        /// <returns>关卡进度存档数据</returns>
        public static LevelProgressSaveDataComponent GetCurrentLevelProgress(SaveManagerComponent saveManager)
        {
            SaveDataRoot saveDataRoot = GetCurrentDataRoot(saveManager);
            return saveDataRoot == null ? null : EnsureLevelProgressData(saveDataRoot);
        }

        /// <summary>
        /// 记录当前进入的关卡并保存
        /// </summary>
        /// <param name="saveManager">存档管理器</param>
        /// <param name="gridConfigId">Grid 配置 id</param>
        /// <returns>保存结果</returns>
        public static async ETTask<SaveResult> SetCurrentLevelAsync(SaveManagerComponent saveManager, int gridConfigId)
        {
            SaveDataRoot saveDataRoot = await EnsureGameplaySaveAsync(saveManager);
            if (saveDataRoot == null)
            {
                return SaveResult.Failed;
            }

            LevelProgressSaveDataComponent progressData = EnsureLevelProgressData(saveDataRoot);
            progressData.SetCurrentLevel(gridConfigId);
            saveDataRoot.GetBasicData()?.SetProgress("Test", gridConfigId);
            saveDataRoot.MarkModified();
            return await saveManager.SaveCurrentAsync();
        }

        /// <summary>
        /// 记录关卡通关并保存
        /// </summary>
        /// <param name="saveManager">存档管理器</param>
        /// <param name="gridConfigId">Grid 配置 id</param>
        /// <returns>保存结果</returns>
        public static async ETTask<SaveResult> PassLevelAsync(SaveManagerComponent saveManager, int gridConfigId)
        {
            SaveDataRoot saveDataRoot = await EnsureGameplaySaveAsync(saveManager);
            if (saveDataRoot == null)
            {
                return SaveResult.Failed;
            }

            LevelProgressSaveDataComponent progressData = EnsureLevelProgressData(saveDataRoot);
            progressData.PassLevel(gridConfigId);
            saveDataRoot.GetBasicData()?.SetProgress("Test", gridConfigId);
            saveDataRoot.MarkModified();
            return await saveManager.SaveCurrentAsync();
        }

        /// <summary>
        /// 获取当前槽位已经加载的完整存档数据根
        /// </summary>
        /// <param name="saveManager">存档管理器</param>
        /// <returns>完整存档数据根</returns>
        private static SaveDataRoot GetCurrentDataRoot(SaveManagerComponent saveManager)
        {
            if (saveManager == null || string.IsNullOrWhiteSpace(saveManager.CurrentSlotId))
            {
                return null;
            }

            return saveManager.GetSlot(saveManager.CurrentSlotId)?.GetDataRoot();
        }

        /// <summary>
        /// 加载当前槽位或默认槽位
        /// </summary>
        /// <param name="saveManager">存档管理器</param>
        /// <returns>完整存档数据根</returns>
        private static async ETTask<SaveDataRoot> LoadCurrentOrDefaultAsync(SaveManagerComponent saveManager)
        {
            if (!string.IsNullOrWhiteSpace(saveManager.CurrentSlotId))
            {
                SaveDataRoot currentDataRoot = await saveManager.LoadSaveAsync(saveManager.CurrentSlotId);
                if (currentDataRoot != null)
                {
                    return currentDataRoot;
                }
            }

            if (saveManager.GetSlot(DefaultSlotId) != null)
            {
                return await saveManager.LoadSaveAsync(DefaultSlotId);
            }

            return null;
        }

        /// <summary>
        /// 确保存档数据根拥有关卡进度组件
        /// </summary>
        /// <param name="saveDataRoot">完整存档数据根</param>
        /// <returns>关卡进度存档数据</returns>
        private static LevelProgressSaveDataComponent EnsureLevelProgressData(SaveDataRoot saveDataRoot)
        {
            LevelProgressSaveDataComponent progressData = saveDataRoot.GetComponent<LevelProgressSaveDataComponent>();
            if (progressData == null)
            {
                progressData = saveDataRoot.AddComponent<LevelProgressSaveDataComponent>();
            }

            progressData.EnsureRuntimeState();
            return progressData;
        }
    }
}
