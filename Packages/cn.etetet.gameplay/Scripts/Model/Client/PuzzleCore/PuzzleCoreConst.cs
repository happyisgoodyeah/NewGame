namespace ET.Client
{
    /// <summary>
    /// PuzzleCore 运行时使用的固定常量。
    /// </summary>
    public static class PuzzleCoreConst
    {
        /// <summary>
        /// 当前场景下默认 Grid 的固定实体 Id。
        /// </summary>
        public const long DefaultGridId = 1;

        /// <summary>
        /// 当前调试阶段固定读取的默认 Grid 配置 id。
        /// </summary>
        public const int DefaultGridConfigId = 1001;

        /// <summary>
        /// 当前场景下动态创建 Puzzle 实体时使用的起始 Id。
        /// </summary>
        public const long DefaultPuzzleEntityIdStart = 1000;
    }
}
