namespace ET.Client
{
    /// <summary>
    /// PuzzleCore 场景表现层使用的固定常量。
    /// </summary>
    public static class PuzzleViewConst
    {
        /// <summary>
        /// Grid 每个 Slot 在场景中的间距。
        /// </summary>
        public const float GridCellSize = 3f;

        /// <summary>
        /// 左键按下后判定为正式拖拽的屏幕位移阈值。
        /// </summary>
        public const float DragStartScreenThreshold = 12f;

        /// <summary>
        /// 拖拽中拼图主图临时抬高到的排序值。
        /// </summary>
        public const int DraggingSortingOrder = 100;

        /// <summary>
        /// 非拖拽状态下拼图主图的默认排序值。
        /// </summary>
        public const int DefaultSortingOrder = 0;
    }
}
