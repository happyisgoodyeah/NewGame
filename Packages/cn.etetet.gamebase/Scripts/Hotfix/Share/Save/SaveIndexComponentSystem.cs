using System;

namespace ET
{
    [EntitySystemOf(typeof(SaveIndexComponent))]
    public static partial class SaveIndexComponentSystem
    {
        /// <summary>
        /// 初始化存档索引默认值
        /// </summary>
        /// <param name="self">存档索引</param>
        [EntitySystem]
        private static void Awake(this SaveIndexComponent self)
        {
            self.DataVersion = SaveConst.CurrentDataVersion;
            self.CreateTime = DateTime.Now;
            self.UpdateTime = self.CreateTime;
        }

        /// <summary>
        /// 按槽位标识获取存档槽位
        /// </summary>
        /// <param name="self">存档索引</param>
        /// <param name="slotId">槽位标识</param>
        /// <returns>匹配的存档槽位</returns>
        public static SaveSlot GetSlot(this SaveIndexComponent self, string slotId)
        {
            if (slotId.IsNullOrWhiteSpace() || self.ChildrenCount() == 0)
            {
                return null;
            }

            foreach (Entity child in self.Children.Values)
            {
                if (child is SaveSlot slot && slot.SlotId == slotId)
                {
                    return slot;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取下一个槽位排序值
        /// </summary>
        /// <param name="self">存档索引</param>
        /// <returns>排序值</returns>
        public static int NextSortOrder(this SaveIndexComponent self)
        {
            int maxSortOrder = 0;
            if (self.ChildrenCount() == 0)
            {
                return maxSortOrder;
            }

            foreach (Entity child in self.Children.Values)
            {
                if (child is SaveSlot slot && slot.SortOrder >= maxSortOrder)
                {
                    maxSortOrder = slot.SortOrder + 1;
                }
            }

            return maxSortOrder;
        }
    }
}
