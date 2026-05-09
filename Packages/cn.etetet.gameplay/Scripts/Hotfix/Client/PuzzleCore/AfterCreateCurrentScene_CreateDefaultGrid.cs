namespace ET.Client
{
    /// <summary>
    /// 当前场景创建完成后，立刻按默认 Grid 配置生成 PuzzleCore 数据。
    /// </summary>
    [Event(SceneType.Current)]
    public class AfterCreateCurrentScene_CreateDefaultGrid : AEvent<Scene, AfterCreateCurrentScene>
    {
        /// <summary>
        /// 根据默认 Grid 配置创建当前关卡的 Grid、Grid Slot 和 Puzzle。
        /// </summary>
        protected override async ETTask Run(Scene scene, AfterCreateCurrentScene args)
        {
            PuzzleBootstrapHelper.CreateDefaultGrid(scene);
            await ETTask.CompletedTask;
        }
    }
}
