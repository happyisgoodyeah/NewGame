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
        /// 加载 Gameplay 存档，不存在槽位时创建默认存档
        /// </summary>
        /// <param name="saveManager">存档管理器</param>
        /// <returns>已加载的完整存档数据根</returns>
        public static async ETTask<SaveDataRoot> LoadGameplaySaveAsync(SaveManagerComponent saveManager)
        {
            if (saveManager == null || !saveManager.IsInitialized)
            {
                return null;
            }

            SaveDataRoot saveDataRoot = await LoadCurrentOrDefaultAsync(saveManager);
            if (saveDataRoot == null)
            {
                return null;
            }

            LevelProgressSaveDataComponent progressData = saveDataRoot.GetComponent<LevelProgressSaveDataComponent>();
            if (progressData == null)
            {
                Log.Error($"Gameplay 存档缺少关卡进度数据: {saveDataRoot.SlotId}");
                return null;
            }

            PuzzleArchiveSaveDataComponent archiveData = saveDataRoot.GetComponent<PuzzleArchiveSaveDataComponent>();
            if (archiveData == null)
            {
                Log.Error($"Gameplay 存档缺少拼图图鉴数据: {saveDataRoot.SlotId}");
                return null;
            }

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
            return saveDataRoot?.GetComponent<LevelProgressSaveDataComponent>();
        }

        /// <summary>
        /// 获取当前已加载存档中的拼图图鉴
        /// </summary>
        /// <param name="saveManager">存档管理器</param>
        /// <returns>拼图图鉴存档数据</returns>
        public static PuzzleArchiveSaveDataComponent GetCurrentPuzzleArchive(SaveManagerComponent saveManager)
        {
            SaveDataRoot saveDataRoot = GetCurrentDataRoot(saveManager);
            return saveDataRoot?.GetComponent<PuzzleArchiveSaveDataComponent>();
        }

        /// <summary>
        /// 记录关卡通关、解锁关卡内拼图并保存
        /// </summary>
        /// <param name="saveManager">存档管理器</param>
        /// <param name="gridConfigId">Grid 配置 id</param>
        /// <returns>保存结果</returns>
        public static async ETTask<SaveResult> CompleteLevelAsync(SaveManagerComponent saveManager, int gridConfigId)
        {
            SaveDataRoot saveDataRoot = await LoadGameplaySaveAsync(saveManager);
            if (saveDataRoot == null)
            {
                return SaveResult.Failed;
            }

            LevelProgressSaveDataComponent progressData = saveDataRoot.GetComponent<LevelProgressSaveDataComponent>();
            if (progressData == null)
            {
                Log.Error($"Gameplay 存档缺少关卡进度数据: {saveDataRoot.SlotId}");
                return SaveResult.Failed;
            }

            PuzzleArchiveSaveDataComponent archiveData = saveDataRoot.GetComponent<PuzzleArchiveSaveDataComponent>();
            if (archiveData == null)
            {
                Log.Error($"Gameplay 存档缺少拼图图鉴数据: {saveDataRoot.SlotId}");
                return SaveResult.Failed;
            }

            progressData.PassLevel(gridConfigId);
            archiveData.UnlockPuzzlesByLevel(gridConfigId);
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
            if (saveManager == null || saveManager.CurrentSlotId.IsNullOrWhiteSpace())
            {
                return null;
            }

            return saveManager.GetSlot(saveManager.CurrentSlotId)?.GetDataRoot();
        }

        /// <summary>
        /// 加载当前槽位、默认槽位或创建默认槽位
        /// </summary>
        /// <param name="saveManager">存档管理器</param>
        /// <returns>完整存档数据根</returns>
        private static async ETTask<SaveDataRoot> LoadCurrentOrDefaultAsync(SaveManagerComponent saveManager)
        {
            string currentSlotId = saveManager.CurrentSlotId;
            SaveDataRoot saveDataRoot = await LoadSlotDataRootAsync(saveManager, currentSlotId);
            if (saveDataRoot != null)
            {
                return saveDataRoot;
            }

            if (currentSlotId != DefaultSlotId)
            {
                saveDataRoot = await LoadSlotDataRootAsync(saveManager, DefaultSlotId);
                if (saveDataRoot != null)
                {
                    return saveDataRoot;
                }
            }

            SaveSlot saveSlot = await saveManager.CreateSaveAsync(DefaultSlotId, DefaultDisplayName, SaveSlotType.Auto);
            return await CreateGameplayDataRootAsync(saveManager, saveSlot);
        }

        /// <summary>
        /// 加载指定槽位的数据根
        /// </summary>
        /// <param name="saveManager">存档管理器</param>
        /// <param name="slotId">槽位标识</param>
        /// <returns>完整存档数据根</returns>
        private static async ETTask<SaveDataRoot> LoadSlotDataRootAsync(SaveManagerComponent saveManager, string slotId)
        {
            if (slotId.IsNullOrWhiteSpace())
            {
                return null;
            }

            SaveSlot saveSlot = saveManager.GetSlot(slotId);
            if (saveSlot == null)
            {
                return null;
            }

            SaveDataRoot saveDataRoot = saveSlot.GetDataRoot();
            return saveDataRoot ?? await saveManager.LoadSaveAsync(slotId);
        }

        /// <summary>
        /// 初始化新建 Gameplay 存档数据
        /// </summary>
        /// <param name="saveManager">存档管理器</param>
        /// <param name="saveSlot">新建槽位</param>
        /// <returns>完整存档数据根</returns>
        private static async ETTask<SaveDataRoot> CreateGameplayDataRootAsync(SaveManagerComponent saveManager, SaveSlot saveSlot)
        {
            SaveDataRoot saveDataRoot = saveSlot?.GetDataRoot();
            if (saveDataRoot == null)
            {
                return null;
            }

            saveDataRoot.AddComponent<LevelProgressSaveDataComponent>();
            saveDataRoot.AddComponent<PuzzleArchiveSaveDataComponent>();
            await saveManager.SaveCurrentAsync();
            return saveDataRoot;
        }
    }
}
