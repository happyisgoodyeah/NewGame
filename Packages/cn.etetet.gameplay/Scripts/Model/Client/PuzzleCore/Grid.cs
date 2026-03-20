namespace ET.Client
{
    /// <summary>
    /// 运行时棋盘实体，持有棋盘尺寸和所有棋盘格 Slot。
    /// </summary>
    [ChildOf(typeof(Scene))]
    public class Grid : Entity, IAwake<int, int>
    {
        /// <summary>
        /// 棋盘列数。
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// 棋盘行数。
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// 当前棋盘中可放置格子的数量。
        /// </summary>
        public int PlaceableCount { get; set; }

        /// <summary>
        /// 当前棋盘中已经被拼图占用的可放置格子数量。
        /// </summary>
        public int OccupiedCount { get; set; }
    }
}
