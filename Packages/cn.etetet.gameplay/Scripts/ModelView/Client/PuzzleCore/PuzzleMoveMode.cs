namespace ET.Client
{
    /// <summary>
    /// Puzzle 表现层当前使用的移动模式。
    /// </summary>
    public enum PuzzleMoveMode : byte
    {
        /// <summary>
        /// 与 Grid 分离时的自由跟随模式。
        /// </summary>
        FreeFollow = 0,

        /// <summary>
        /// 与 Grid 接触后的按格移动模式。
        /// </summary>
        GridSnap = 1,
    }
}
