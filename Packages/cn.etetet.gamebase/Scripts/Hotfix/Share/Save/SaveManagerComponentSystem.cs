using System;

namespace ET
{
    [EntitySystemOf(typeof(SaveManagerComponent))]
    public static partial class SaveManagerComponentSystem
    {
        /// <summary>
        /// 初始化存档管理器运行时状态
        /// </summary>
        /// <param name="self">存档管理器</param>
        [EntitySystem]
        private static void Awake(this SaveManagerComponent self)
        {
            self.SaveRootDirectory = string.Empty;
            self.GameVersion = string.Empty;
            self.CurrentSlotId = string.Empty;
            self.IsInitialized = false;
        }

        /// <summary>
        /// 清理存档管理器运行时状态
        /// </summary>
        /// <param name="self">存档管理器</param>
        [EntitySystem]
        private static void Destroy(this SaveManagerComponent self)
        {
            self.SaveRootDirectory = string.Empty;
            self.GameVersion = string.Empty;
            self.CurrentSlotId = string.Empty;
            self.IsInitialized = false;
        }

        /// <summary>
        /// 初始化存档管理器
        /// </summary>
        /// <param name="self">存档管理器</param>
        /// <param name="saveRootDirectory">存档根目录</param>
        /// <param name="gameVersion">游戏版本</param>
        /// <returns>初始化结果</returns>
        public static async ETTask<SaveResult> Initialize(this SaveManagerComponent self, string saveRootDirectory, string gameVersion)
        {
            if (string.IsNullOrWhiteSpace(saveRootDirectory))
            {
                Log.Error("存档根目录为空");
                return SaveResult.Failed;
            }

            self.SaveRootDirectory = saveRootDirectory;
            self.GameVersion = gameVersion ?? string.Empty;

            self.EnsureValidationDispatcher();
            await self.LoadIndexAsync();

            SaveIndexComponent saveIndex = self.GetIndex();
            self.CurrentSlotId = saveIndex.CurrentSlotId;
            self.IsInitialized = true;
            self.InitializedTime = DateTime.Now;

            return SaveResult.Success;
        }

        /// <summary>
        /// 创建新存档槽位
        /// </summary>
        /// <param name="self">存档管理器</param>
        /// <param name="slotId">槽位标识</param>
        /// <param name="displayName">展示名称</param>
        /// <param name="slotType">槽位类型</param>
        /// <returns>新建的存档槽位</returns>
        public static async ETTask<SaveSlot> CreateSaveAsync(
            this SaveManagerComponent self,
            string slotId = null,
            string displayName = null,
            SaveSlotType slotType = SaveSlotType.Manual)
        {
            if (!self.CheckInitialized())
            {
                return null;
            }

            SaveIndexComponent saveIndex = self.GetIndex();
            string finalSlotId = string.IsNullOrWhiteSpace(slotId) ? self.GenerateSlotId() : slotId;
            if (saveIndex.GetSlot(finalSlotId) != null)
            {
                Log.Error($"存档槽位已存在: {finalSlotId}");
                return null;
            }

            SaveSlot saveSlot = saveIndex.AddChild<SaveSlot, string, SaveSlotType>(finalSlotId, slotType);
            SaveDataHeaderComponent header = saveSlot.GetHeader();
            header.DisplayName = string.IsNullOrWhiteSpace(displayName) ? SaveConst.DefaultDisplayName : displayName;
            header.GameVersion = self.GameVersion;

            SaveDataRoot saveDataRoot = saveSlot.AddChild<SaveDataRoot, string>(finalSlotId);
            saveDataRoot.GetBasicData().SetPlayer(finalSlotId, header.DisplayName);

            self.SetCurrentSlot(finalSlotId);
            SaveResult result = await self.WriteSlotAsync(saveSlot, saveDataRoot);
            return result == SaveResult.Success ? saveSlot : null;
        }

        /// <summary>
        /// 加载指定存档槽位的数据
        /// </summary>
        /// <param name="self">存档管理器</param>
        /// <param name="slotId">槽位标识</param>
        /// <returns>完整存档数据根</returns>
        public static async ETTask<SaveDataRoot> LoadSaveAsync(this SaveManagerComponent self, string slotId)
        {
            if (!self.CheckInitialized())
            {
                return null;
            }

            SaveSlot saveSlot = self.GetSlot(slotId);
            if (saveSlot == null)
            {
                Log.Error($"存档槽位不存在: {slotId}");
                return null;
            }

            SaveDataRoot saveDataRoot = await self.ReadDataRootAsync(saveSlot);
            if (saveDataRoot == null)
            {
                return null;
            }

            saveSlot.ClearDataRoot();
            saveSlot.AddChild(saveDataRoot);
            saveSlot.IsDataLoaded = true;
            saveSlot.LastLoadedTime = DateTime.Now;
            self.SetCurrentSlot(slotId);

            return saveDataRoot;
        }

        /// <summary>
        /// 保存当前槽位数据
        /// </summary>
        /// <param name="self">存档管理器</param>
        /// <returns>保存结果</returns>
        public static async ETTask<SaveResult> SaveCurrentAsync(this SaveManagerComponent self)
        {
            if (string.IsNullOrWhiteSpace(self.CurrentSlotId))
            {
                Log.Error("当前存档槽位为空");
                return SaveResult.SlotNotFound;
            }

            return await self.SaveAsync(self.CurrentSlotId);
        }

        /// <summary>
        /// 保存指定槽位数据
        /// </summary>
        /// <param name="self">存档管理器</param>
        /// <param name="slotId">槽位标识</param>
        /// <returns>保存结果</returns>
        public static async ETTask<SaveResult> SaveAsync(this SaveManagerComponent self, string slotId)
        {
            if (!self.CheckInitialized())
            {
                return SaveResult.NotInitialized;
            }

            SaveSlot saveSlot = self.GetSlot(slotId);
            if (saveSlot == null)
            {
                return SaveResult.SlotNotFound;
            }

            SaveDataRoot saveDataRoot = saveSlot.GetDataRoot();
            if (saveDataRoot == null)
            {
                saveDataRoot = await self.ReadDataRootAsync(saveSlot);
                if (saveDataRoot == null)
                {
                    return SaveResult.Failed;
                }

                saveSlot.AddChild(saveDataRoot);
            }

            return await self.WriteSlotAsync(saveSlot, saveDataRoot);
        }

        /// <summary>
        /// 删除指定存档槽位
        /// </summary>
        /// <param name="self">存档管理器</param>
        /// <param name="slotId">槽位标识</param>
        /// <returns>删除结果</returns>
        public static async ETTask<SaveResult> DeleteSaveAsync(this SaveManagerComponent self, string slotId)
        {
            if (!self.CheckInitialized())
            {
                return SaveResult.NotInitialized;
            }

            SaveSlot saveSlot = self.GetSlot(slotId);
            if (saveSlot == null)
            {
                return SaveResult.SlotNotFound;
            }

            string slotDirectory = self.GetSlotDirectory(slotId);
            saveSlot.Dispose();

            if (self.CurrentSlotId == slotId)
            {
                self.SetCurrentSlot(string.Empty);
            }

            self.DeleteSlotDirectory(slotDirectory);
            await self.SaveIndexAsync();
            return SaveResult.Success;
        }

        /// <summary>
        /// 获取存档索引组件
        /// </summary>
        /// <param name="self">存档管理器</param>
        /// <returns>存档索引组件</returns>
        public static SaveIndexComponent GetIndex(this SaveManagerComponent self)
        {
            return self.GetComponent<SaveIndexComponent>();
        }

        /// <summary>
        /// 获取指定存档槽位
        /// </summary>
        /// <param name="self">存档管理器</param>
        /// <param name="slotId">槽位标识</param>
        /// <returns>存档槽位</returns>
        public static SaveSlot GetSlot(this SaveManagerComponent self, string slotId)
        {
            return self.GetIndex()?.GetSlot(slotId);
        }

        /// <summary>
        /// 获取校验分发组件
        /// </summary>
        /// <param name="self">存档管理器</param>
        /// <returns>校验分发组件</returns>
        public static SaveValidationDispatcherComponent GetValidationDispatcher(this SaveManagerComponent self)
        {
            return self.GetComponent<SaveValidationDispatcherComponent>();
        }

        /// <summary>
        /// 确保校验分发组件已经挂载
        /// </summary>
        /// <param name="self">存档管理器</param>
        private static void EnsureValidationDispatcher(this SaveManagerComponent self)
        {
            if (self.GetValidationDispatcher() == null)
            {
                self.AddComponent<SaveValidationDispatcherComponent>();
            }
        }

        /// <summary>
        /// 检查存档管理器是否已经初始化
        /// </summary>
        /// <param name="self">存档管理器</param>
        /// <returns>是否已经初始化</returns>
        private static bool CheckInitialized(this SaveManagerComponent self)
        {
            if (self.IsInitialized)
            {
                return true;
            }

            Log.Error("存档管理器尚未初始化");
            return false;
        }

        /// <summary>
        /// 设置当前槽位
        /// </summary>
        /// <param name="self">存档管理器</param>
        /// <param name="slotId">槽位标识</param>
        private static void SetCurrentSlot(this SaveManagerComponent self, string slotId)
        {
            self.CurrentSlotId = slotId ?? string.Empty;

            SaveIndexComponent saveIndex = self.GetIndex();
            if (saveIndex == null)
            {
                return;
            }

            saveIndex.CurrentSlotId = self.CurrentSlotId;
            saveIndex.UpdateTime = DateTime.Now;
        }

        /// <summary>
        /// 生成默认槽位标识
        /// </summary>
        /// <param name="self">存档管理器</param>
        /// <returns>槽位标识</returns>
        private static string GenerateSlotId(this SaveManagerComponent self)
        {
            return $"slot_{IdGenerater.Instance.GenerateId()}";
        }
    }
}
