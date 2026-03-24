using System;

namespace ET.Client
{
    [EntitySystemOf(typeof(Grid))]
    public static partial class GridSystem
    {
        [EntitySystem]
        private static void Awake(this Grid self, int width, int height)
        {
            self.Width = width;
            self.Height = height;
            self.PlaceableCount = 0;
            self.OccupiedCount = 0;
        }

        /// <summary>
        /// 判断指定坐标是否落在棋盘边界内。
        /// </summary>
        public static bool Contains(this Grid self, int x, int y)
        {
            return x >= 0 && x < self.Width && y >= 0 && y < self.Height;
        }

        /// <summary>
        /// 按局部坐标获取 Grid 下面的 Slot。
        /// </summary>
        public static Slot GetSlot(this Grid self, int x, int y)
        {
            return self.GetChild<Slot>(SlotSystem.ToSlotId(x, y));
        }

        /// <summary>
        /// 向 Grid 中添加一个棋盘格 Slot，并在数据创建完成后通知表现层生成 SlotView。
        /// </summary>
        public static Slot AddSlot(this Grid self, int x, int y, SlotType kind)
        {
            if (!self.Contains(x, y))
            {
                throw new ArgumentOutOfRangeException($"grid slot out of range: ({x}, {y})");
            }

            if (kind != SlotType.GridBlocked && kind != SlotType.GridPlaceable)
            {
                throw new ArgumentException($"grid slot kind is invalid: {kind}");
            }

            long slotId = SlotSystem.ToSlotId(x, y);
            if (self.GetChild<Slot>(slotId) != null)
            {
                throw new Exception($"grid slot already exists: ({x}, {y})");
            }

            Slot slot = self.AddChildWithId<Slot, int, int, SlotType>(slotId, x, y, kind);
            slot.AddComponent<GridSlotStateComponent>();
            EventSystem.Instance.Publish(self.Scene(), new AfterCreateSlot() { Slot = slot });
            self.RefreshStatistics();
            return slot;
        }

        /// <summary>
        /// 重新统计棋盘可放置格和已占用格数量。
        /// </summary>
        public static void RefreshStatistics(this Grid self)
        {
            int placeableCount = 0;
            int occupiedCount = 0;

            if (self.ChildrenCount() > 0)
            {
                foreach (Entity child in self.Children.Values)
                {
                    Slot slot = child as Slot;
                    if (slot == null || slot.Kind != SlotType.GridPlaceable)
                    {
                        continue;
                    }

                    ++placeableCount;
                    if (slot.GetBindingPuzzle() != null)
                    {
                        ++occupiedCount;
                    }
                }
            }

            self.PlaceableCount = placeableCount;
            self.OccupiedCount = occupiedCount;
        }
    }
}
