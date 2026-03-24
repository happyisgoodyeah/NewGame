namespace ET.Client
{
    /// <summary>
    /// Grid 数据创建完成后抛出的事件。
    /// </summary>
    public struct AfterCreateGrid
    {
        /// <summary>
        /// 本次创建完成的 Grid。
        /// </summary>
        public Grid Grid;
    }

    /// <summary>
    /// Puzzle 数据创建完成后抛出的事件。
    /// </summary>
    public struct AfterCreatePuzzle
    {
        /// <summary>
        /// 本次创建完成的 Puzzle。
        /// </summary>
        public Puzzle Puzzle;
    }

    /// <summary>
    /// Slot 数据创建完成后抛出的事件。
    /// </summary>
    public struct AfterCreateSlot
    {
        /// <summary>
        /// 本次创建完成的 Slot。
        /// </summary>
        public Slot Slot;
    }
}
