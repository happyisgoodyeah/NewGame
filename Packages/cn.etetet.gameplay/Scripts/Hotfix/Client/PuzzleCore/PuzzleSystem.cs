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
            self.InitialWorldPositionX = 0f;
            self.InitialWorldPositionY = 0f;
            self.InitialWorldPositionZ = 0f;
            self.Rotation = PuzzleRotation.Rotate0;
            self.State = PuzzleState.Tray;
            self.AnchorX = 0;
            self.AnchorY = 0;
            self.OriginSlotId = 0;
        }

        /// <summary>
        /// 按局部坐标获取 Puzzle 下面的 Slot。
        /// </summary>
        public static Slot GetSlot(this Puzzle self, int x, int y)
        {
            return self.GetChild<Slot>(SlotSystem.ToSlotId(x, y));
        }

        /// <summary>
        /// 获取当前 Puzzle 的原点 Slot。
        /// </summary>
        /// <param name="self">要查询的 Puzzle。</param>
        /// <returns>原点 Slot；若不存在则返回空。</returns>
        public static Slot GetOriginSlot(this Puzzle self)
        {
            if (self.OriginSlotId != 0)
            {
                Slot originSlot = self.GetChild<Slot>(self.OriginSlotId);
                if (originSlot != null)
                {
                    return originSlot;
                }
            }

            return self.GetSlot(0, 0);
        }

        /// <summary>
        /// 向 Puzzle 中添加一个形状格 Slot，并发布 Slot 数据创建完成事件。
        /// </summary>
        public static Slot AddSlot(this Puzzle self, int x, int y)
        {
            long slotId = SlotSystem.ToSlotId(x, y);
            if (self.GetChild<Slot>(slotId) != null)
            {
                throw new Exception($"puzzle slot already exists: ({x}, {y})");
            }

            Slot slot = self.AddChildWithId<Slot, int, int, SlotType>(slotId, x, y, SlotType.PuzzleFilled);
            if (x == 0 && y == 0)
            {
                self.OriginSlotId = slot.Id;
            }

            EventSystem.Instance.Publish(self.Scene(), new AfterCreateSlot() { Slot = slot });
            return slot;
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

        /// <summary>
        /// 将指定 Puzzle Slot 的相对原点坐标转换为当前旋转状态下的离散偏移。
        /// </summary>
        /// <param name="self">要计算偏移的 Puzzle。</param>
        /// <param name="slot">要计算的 Puzzle Slot。</param>
        /// <param name="offsetX">返回旋转后的 X 偏移。</param>
        /// <param name="offsetY">返回旋转后的 Y 偏移。</param>
        public static void GetRotatedOffset(this Puzzle self, Slot slot, out int offsetX, out int offsetY)
        {
            RotateOffset(slot.X, slot.Y, self.Rotation, out offsetX, out offsetY);
        }

        /// <summary>
        /// 按当前离散旋转状态计算局部坐标的旋转结果。
        /// </summary>
        /// <param name="x">原始 X 坐标。</param>
        /// <param name="y">原始 Y 坐标。</param>
        /// <param name="rotation">当前离散旋转状态。</param>
        /// <param name="offsetX">返回旋转后的 X 坐标。</param>
        /// <param name="offsetY">返回旋转后的 Y 坐标。</param>
        private static void RotateOffset(int x, int y, PuzzleRotation rotation, out int offsetX, out int offsetY)
        {
            switch (rotation)
            {
                case PuzzleRotation.Rotate90:
                    offsetX = -y;
                    offsetY = x;
                    return;
                case PuzzleRotation.Rotate180:
                    offsetX = -x;
                    offsetY = -y;
                    return;
                case PuzzleRotation.Rotate270:
                    offsetX = y;
                    offsetY = -x;
                    return;
                default:
                    offsetX = x;
                    offsetY = y;
                    return;
            }
        }
    }
}
