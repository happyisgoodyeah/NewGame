namespace ET.Client
{
    /// <summary>
    /// 当前场景创建完成后，立刻生成默认的 Grid 和 Slot 数据。
    /// </summary>
    [Event(SceneType.Current)]
    public class AfterCreateCurrentScene_CreateDefaultGrid : AEvent<Scene, AfterCreateCurrentScene>
    {
        protected override async ETTask Run(Scene scene, AfterCreateCurrentScene args)
        {
            PuzzleBootstrapHelper.CreateDefaultGrid(scene);
            await ETTask.CompletedTask;
        }
    }
}
