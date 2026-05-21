using System;

namespace ET
{
    [EntitySystemOf(typeof(SaveSlot))]
    public static partial class SaveSlotSystem
    {
        /// <summary>
        /// 初始化存档槽位与头信息
        /// </summary>
        /// <param name="self">存档槽位</param>
        /// <param name="slotId">槽位标识</param>
        /// <param name="slotType">槽位类型</param>
        [EntitySystem]
        private static void Awake(this SaveSlot self, string slotId, SaveSlotType slotType)
        {
            self.SlotId = slotId;
            self.SlotType = slotType;
            self.SortOrder = self.GetParent<SaveIndexComponent>().NextSortOrder();

            SaveDataHeaderComponent header = self.AddComponent<SaveDataHeaderComponent>();
            header.SlotId = slotId;
            header.SlotType = slotType;
            header.CreateTime = DateTime.Now;
            header.UpdateTime = header.CreateTime;
        }

        /// <summary>
        /// 获取槽位头信息
        /// </summary>
        /// <param name="self">存档槽位</param>
        /// <returns>槽位头信息</returns>
        public static SaveDataHeaderComponent GetHeader(this SaveSlot self)
        {
            return self.GetComponent<SaveDataHeaderComponent>();
        }

        /// <summary>
        /// 获取已加载的完整存档数据根
        /// </summary>
        /// <param name="self">存档槽位</param>
        /// <returns>完整存档数据根</returns>
        public static SaveDataRoot GetDataRoot(this SaveSlot self)
        {
            if (self.ChildrenCount() == 0)
            {
                return null;
            }

            foreach (Entity child in self.Children.Values)
            {
                if (child is SaveDataRoot saveDataRoot)
                {
                    return saveDataRoot;
                }
            }

            return null;
        }

        /// <summary>
        /// 清理槽位下已加载的完整存档数据
        /// </summary>
        /// <param name="self">存档槽位</param>
        public static void ClearDataRoot(this SaveSlot self)
        {
            SaveDataRoot saveDataRoot = self.GetDataRoot();
            if (saveDataRoot == null)
            {
                return;
            }

            saveDataRoot.Dispose();
            self.IsDataLoaded = false;
        }

        /// <summary>
        /// 将头信息复制到当前槽位
        /// </summary>
        /// <param name="self">存档槽位</param>
        /// <param name="source">来源头信息</param>
        public static void CopyHeaderFrom(this SaveSlot self, SaveDataHeaderComponent source)
        {
            SaveDataHeaderComponent header = self.GetHeader();
            header.SlotId = source.SlotId;
            header.DisplayName = source.DisplayName;
            header.SlotType = source.SlotType;
            header.DataVersion = source.DataVersion;
            header.GameVersion = source.GameVersion;
            header.CreateTime = source.CreateTime;
            header.UpdateTime = source.UpdateTime;
            header.PlayTimeSeconds = source.PlayTimeSeconds;
            header.DataSize = source.DataSize;
            header.Description = source.Description;
        }
    }
}
