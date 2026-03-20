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
            self.OccupyPuzzleId = 0;
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
            return self.OccupyPuzzleId != 0;
        }

        /// <summary>
        /// 记录当前格子被哪块拼图占用。
        /// </summary>
        public static void Occupy(this Slot self, Puzzle puzzle)
        {
            self.OccupyPuzzleId = puzzle?.Id ?? 0;
        }

        /// <summary>
        /// 清空当前格子的占用记录。
        /// </summary>
        public static void ClearOccupy(this Slot self)
        {
            self.OccupyPuzzleId = 0;
        }
    }
}
