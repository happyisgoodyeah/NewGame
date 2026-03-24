namespace ET.Client
{
    /// <summary>
    /// 当前场景中 PuzzleCore 数据的快捷访问方法。
    /// </summary>
    public static partial class PuzzleSceneSystem
    {
        /// <summary>
        /// 获取当前场景下默认生成的 Grid。
        /// </summary>
        public static Grid GetGrid(this Scene self)
        {
            return self.GetChild<Grid>(PuzzleCoreConst.DefaultGridId);
        }

        /// <summary>
        /// 按实体 id 获取当前场景下的 Puzzle。
        /// </summary>
        public static Puzzle GetPuzzle(this Scene self, long puzzleId)
        {
            return self.GetChild<Puzzle>(puzzleId);
        }
    }
}
