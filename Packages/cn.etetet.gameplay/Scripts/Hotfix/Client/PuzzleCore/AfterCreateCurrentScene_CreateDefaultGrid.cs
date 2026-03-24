namespace ET.Client
{
    /// <summary>
    /// 当前场景创建完成后，立刻生成默认的 Grid 和 Puzzle 数据。
    /// </summary>
    [Event(SceneType.Current)]
    public class AfterCreateCurrentScene_CreateDefaultGrid : AEvent<Scene, AfterCreateCurrentScene>
    {
        /// <summary>
        /// 创建默认 Grid 和左右两个默认 Puzzle，后续表现层由数据创建事件驱动生成。
        /// </summary>
        protected override async ETTask Run(Scene scene, AfterCreateCurrentScene args)
        {
            PuzzleBootstrapHelper.CreateDefaultGrid(scene);
            PuzzleBootstrapHelper.CreateDefaultPuzzle(scene, PuzzleCoreConst.DefaultLeftPuzzleId);
            PuzzleBootstrapHelper.CreateDefaultPuzzle(scene, PuzzleCoreConst.DefaultRightPuzzleId);
            await ETTask.CompletedTask;
        }
    }
}
