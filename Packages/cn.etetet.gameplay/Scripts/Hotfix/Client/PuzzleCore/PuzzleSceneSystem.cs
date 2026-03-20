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
    }
}
