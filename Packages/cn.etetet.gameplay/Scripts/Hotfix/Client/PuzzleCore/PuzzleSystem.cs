using System;

namespace ET.Client
{
    [EntitySystemOf(typeof(Puzzle))]
    public static partial class PuzzleSystem
    {
        [EntitySystem]
        private static void Awake(this Puzzle self, int puzzleConfigId)
        {
            self.PuzzleConfigId = puzzleConfigId;
            self.Rotation = PuzzleRotation.Rotate0;
            self.State = PuzzleState.Tray;
            self.AnchorX = 0;
            self.AnchorY = 0;
        }

        /// <summary>
        /// 按局部坐标获取 Puzzle 下面的 Slot。
        /// </summary>
        public static Slot GetSlot(this Puzzle self, int x, int y)
        {
            return self.GetChild<Slot>(SlotSystem.ToSlotId(x, y));
        }

        /// <summary>
        /// 向 Puzzle 中添加一个形状格 Slot。
        /// </summary>
        public static Slot AddSlot(this Puzzle self, int x, int y)
        {
            long slotId = SlotSystem.ToSlotId(x, y);
            if (self.GetChild<Slot>(slotId) != null)
            {
                throw new Exception($"puzzle slot already exists: ({x}, {y})");
            }

            return self.AddChildWithId<Slot, int, int, SlotType>(slotId, x, y, SlotType.PuzzleFilled);
        }

        /// <summary>
        /// 设置拼图锚点在 Grid 坐标系中的位置。
        /// </summary>
        public static void SetAnchor(this Puzzle self, int x, int y)
        {
            self.AnchorX = x;
            self.AnchorY = y;
        }

        /// <summary>
        /// 直接设置拼图旋转状态。
        /// </summary>
        public static void SetRotation(this Puzzle self, PuzzleRotation rotation)
        {
            self.Rotation = rotation;
        }

        /// <summary>
        /// 让拼图顺时针旋转 90 度。
        /// </summary>
        public static void RotateClockwise(this Puzzle self)
        {
            self.Rotation = (PuzzleRotation)(((int)self.Rotation + 1) & 0x3);
        }
    }
}
