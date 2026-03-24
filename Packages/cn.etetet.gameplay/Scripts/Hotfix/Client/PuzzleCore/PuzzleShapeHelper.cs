namespace ET.Client
{
    /// <summary>
    /// Puzzle 形状相关的辅助方法，负责提供原点 Slot 与旋转后的离散偏移。
    /// </summary>
    public static class PuzzleShapeHelper
    {
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
