namespace ET.Client
{
    [EntitySystemOf(typeof(Slot))]
    public static partial class SlotSystem
    {
        [EntitySystem]
        private static void Awake(this Slot self, int x, int y, SlotType kind)
        {
            self.X = x;
            self.Y = y;
            self.Kind = kind;
            self.BindingPuzzle = default;
        }

        /// <summary>
        /// 将二维坐标编码成 Slot 的唯一 Id，便于按坐标查找子节点。
        /// </summary>
        public static long ToSlotId(int x, int y)
        {
            return ((long)(uint)x << 32) | (uint)y;
        }

        /// <summary>
        /// 判断当前 Slot 是否挂在 Grid 下。
        /// </summary>
        public static bool IsGridSlot(this Slot self)
        {
            return self.Parent is Grid;
        }

        /// <summary>
        /// 判断当前 Slot 是否挂在 Puzzle 下。
        /// </summary>
        public static bool IsPuzzleSlot(this Slot self)
        {
            return self.Parent is Puzzle;
        }

        /// <summary>
        /// 判断当前 Slot 是否为不可放置的棋盘格。
        /// </summary>
        public static bool IsBlocked(this Slot self)
        {
            return self.Kind == SlotType.GridBlocked;
        }

        /// <summary>
        /// 判断当前 Slot 是否为可放置的棋盘格。
        /// </summary>
        public static bool IsPlaceable(this Slot self)
        {
            return self.Kind == SlotType.GridPlaceable;
        }

        /// <summary>
        /// 判断当前 Slot 是否为拼图形状格。
        /// </summary>
        public static bool IsFilled(this Slot self)
        {
            return self.Kind == SlotType.PuzzleFilled;
        }

        /// <summary>
        /// 判断当前 Grid Slot 是否已被某块拼图占用。
        /// </summary>
        public static bool IsOccupied(this Slot self)
        {
            return self.GetBindingPuzzle() != null;
        }

        /// <summary>
        /// 获取当前 Grid Slot 绑定的 Puzzle。
        /// </summary>
        /// <param name="self">要查询的 Slot。</param>
        /// <returns>当前绑定的 Puzzle；若未绑定则返回空。</returns>
        public static Puzzle GetBindingPuzzle(this Slot self)
        {
            return self.BindingPuzzle;
        }

        /// <summary>
        /// 判断当前 Grid Slot 是否绑定到指定 Puzzle。
        /// </summary>
        /// <param name="self">要查询的 Slot。</param>
        /// <param name="puzzle">要比对的 Puzzle。</param>
        /// <returns>当前 Slot 是否绑定到指定 Puzzle。</returns>
        public static bool IsBoundToPuzzle(this Slot self, Puzzle puzzle)
        {
            return puzzle != null && self.GetBindingPuzzle() == puzzle;
        }

        /// <summary>
        /// 将当前 Grid Slot 绑定到指定 Puzzle。
        /// </summary>
        /// <param name="self">要绑定的 Slot。</param>
        /// <param name="puzzle">要写入的 Puzzle。</param>
        public static void BindPuzzle(this Slot self, Puzzle puzzle)
        {
            if (puzzle == null)
            {
                self.BindingPuzzle = default;
                return;
            }

            self.BindingPuzzle = puzzle;
        }

        /// <summary>
        /// 清空当前 Grid Slot 的 Puzzle 绑定关系。
        /// </summary>
        /// <param name="self">要清空绑定的 Slot。</param>
        public static void ClearBindingPuzzle(this Slot self)
        {
            self.BindingPuzzle = default;
        }
    }
}
