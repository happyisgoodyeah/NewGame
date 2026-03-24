namespace ET.Client
{
    /// <summary>
    /// PuzzleCore 场景表现层使用的固定常量。
    /// </summary>
    public static class PuzzleViewConst
    {
        /// <summary>
        /// GridView 使用的 prefab 资源路径。
        /// </summary>
        public const string GridPrefabPath = "Prefab/Grid1001";

        /// <summary>
        /// Grid SlotView 使用的 prefab 资源路径。
        /// </summary>
        public const string GridSlotPrefabPath = "Slot1001";

        /// <summary>
        /// Puzzle 预制体内部预期使用的 Slot prefab 路径。
        /// </summary>
        public const string PuzzleSlotPrefabPath = "Slot1000";

        /// <summary>
        /// 默认 PuzzleView 使用的 prefab 资源路径。
        /// </summary>
        public const string DefaultPuzzlePrefabPath = "Prefab/Puzzle1001";

        /// <summary>
        /// Grid 每个 Slot 在场景中的间距。
        /// </summary>
        public const float GridCellSize = 3f;

        /// <summary>
        /// Puzzle 初始摆放到 Grid 两侧时预留的水平间隔。
        /// </summary>
        public const float PuzzleHorizontalMargin = 6f;

        /// <summary>
        /// 左侧 Puzzle 初始摆放的垂直偏移。
        /// </summary>
        public const float LeftPuzzleStartY = 3.5f;

        /// <summary>
        /// 右侧 Puzzle 初始摆放的垂直偏移。
        /// </summary>
        public const float RightPuzzleStartY = -3.5f;
    }
}
