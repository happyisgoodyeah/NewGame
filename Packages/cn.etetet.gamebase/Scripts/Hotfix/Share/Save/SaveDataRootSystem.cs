using System;

namespace ET
{
    [EntitySystemOf(typeof(SaveDataRoot))]
    public static partial class SaveDataRootSystem
    {
        /// <summary>
        /// 初始化完整存档数据根
        /// </summary>
        /// <param name="self">完整存档数据根</param>
        /// <param name="slotId">槽位标识</param>
        [EntitySystem]
        private static void Awake(this SaveDataRoot self, string slotId)
        {
            self.SlotId = slotId;
            self.DataVersion = SaveConst.CurrentDataVersion;
            self.CreateTime = DateTime.Now;
            self.LastModifiedTime = self.CreateTime;
            self.AddComponent<BasicSaveDataComponent>();
        }

        /// <summary>
        /// 获取基础存档数据组件
        /// </summary>
        /// <param name="self">完整存档数据根</param>
        /// <returns>基础存档数据组件</returns>
        public static BasicSaveDataComponent GetBasicData(this SaveDataRoot self)
        {
            return self.GetComponent<BasicSaveDataComponent>();
        }

        /// <summary>
        /// 标记完整存档数据已经发生修改
        /// </summary>
        /// <param name="self">完整存档数据根</param>
        public static void MarkModified(this SaveDataRoot self)
        {
            self.LastModifiedTime = DateTime.Now;
            foreach (Entity component in self.Components.Values)
            {
                if (component is ISaveDataComponent saveDataComponent)
                {
                    saveDataComponent.LastModifiedTime = self.LastModifiedTime;
                }
            }
        }
    }
}
