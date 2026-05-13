namespace ET.Client
{
    /// <summary>
    /// Grid 数据实体创建完成事件。
    /// </summary>
    public struct AfterCreateGrid
    {
        /// <summary>
        /// 本次创建完成的 Grid 数据实体。
        /// </summary>
        public Grid Grid;
    }

    /// <summary>
    /// Puzzle 数据实体创建完成事件。
    /// </summary>
    public struct AfterCreatePuzzle
    {
        /// <summary>
        /// 本次创建完成的 Puzzle 数据实体。
        /// </summary>
        public Puzzle Puzzle;
    }

    /// <summary>
    /// Slot 数据实体创建完成事件。
    /// </summary>
    public struct AfterCreateSlot
    {
        /// <summary>
        /// 本次创建完成的 Slot 数据实体。
        /// </summary>
        public Slot Slot;
    }
}
