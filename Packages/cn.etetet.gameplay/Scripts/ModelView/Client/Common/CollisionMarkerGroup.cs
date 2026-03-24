namespace ET.Client
{
    /// <summary>
    /// PuzzleCore 当前使用的通用碰撞分组常量。
    /// </summary>
    public static class CollisionMarkerGroup
    {
        /// <summary>
        /// Grid 整体接触区域。
        /// </summary>
        public const string GridArea = "GridArea";

        /// <summary>
        /// Grid 中的单个 Slot。
        /// </summary>
        public const string GridSlot = "GridSlot";

        /// <summary>
        /// Puzzle 主体图片区域。
        /// </summary>
        public const string PuzzleBody = "PuzzleBody";

        /// <summary>
        /// Puzzle 内部的原点/形状 Slot。
        /// </summary>
        public const string PuzzleSlot = "PuzzleSlot";
    }
}
