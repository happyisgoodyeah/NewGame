namespace ET.Client
{
    /// <summary>
    /// 当前场景切换完成后，再真正生成 PuzzleCore 的 Scene 表现层对象。
    /// </summary>
    [Event(SceneType.Current)]
    public class SceneChangeFinish_CreatePuzzleCoreViews : AEvent<Scene, SceneChangeFinish>
    {
        /// <summary>
        /// 在 Unity 场景加载完成后，为已有 Grid、Puzzle、Slot 数据补发 View 创建事件。
        /// </summary>
        protected override async ETTask Run(Scene scene, SceneChangeFinish args)
        {
            PuzzleViewEventHelper.PublishCreateViewEvents(scene);
            await ETTask.CompletedTask;
        }
    }
}
