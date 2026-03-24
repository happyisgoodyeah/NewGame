namespace ET.Client
{
    /// <summary>
    /// 拼图当前采用的离散旋转角度。
    /// </summary>
    public enum PuzzleRotation : byte
    {
        /// <summary>
        /// 不旋转。
        /// </summary>
        Rotate0 = 0,

        /// <summary>
        /// 顺时针旋转 90 度。
        /// </summary>
        Rotate90 = 1,

        /// <summary>
        /// 顺时针旋转 180 度。
        /// </summary>
        Rotate180 = 2,

        /// <summary>
        /// 顺时针旋转 270 度。
        /// </summary>
        Rotate270 = 3,
    }

    /// <summary>
    /// 拼图实例当前所处的交互状态。
    /// </summary>
    public enum PuzzleState : byte
    {
        /// <summary>
        /// 拼图还在待选区域中。
        /// </summary>
        Tray = 0,

        /// <summary>
        /// 拼图正在被拖拽。
        /// </summary>
        Dragging = 1,

        /// <summary>
        /// 拼图已经放置到棋盘上。
        /// </summary>
        Placed = 2,
    }

    /// <summary>
    /// Slot 的语义类型，由父节点和类型共同决定用途。
    /// </summary>
    public enum SlotType : byte
    {
        /// <summary>
        /// 未初始化或空值。
        /// </summary>
        None = 0,

        /// <summary>
        /// Grid 上不可放置的格子。
        /// </summary>
        GridBlocked = 1,

        /// <summary>
        /// Grid 上可放置的格子。
        /// </summary>
        GridPlaceable = 2,

        /// <summary>
        /// Puzzle 形状实际占用的格子。
        /// </summary>
        PuzzleFilled = 3,
    }

    /// <summary>
    /// Puzzle 放置判定的结果类型。
    /// </summary>
    public enum PuzzlePlacementCheckResult : byte
    {
        /// <summary>
        /// 判定成功，可以应用放置。
        /// </summary>
        Success = 0,

        /// <summary>
        /// 目标位置超出 Grid 边界。
        /// </summary>
        OutOfGrid = 1,

        /// <summary>
        /// 目标位置存在不可放置格。
        /// </summary>
        Blocked = 2,

        /// <summary>
        /// 目标位置已被其他 Puzzle 占用。
        /// </summary>
        Occupied = 3,

        /// <summary>
        /// 判定所需数据缺失或非法。
        /// </summary>
        Invalid = 4,
    }
}
