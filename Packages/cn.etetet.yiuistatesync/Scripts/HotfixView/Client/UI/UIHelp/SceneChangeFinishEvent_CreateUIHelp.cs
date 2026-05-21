
namespace ET.Client
{
    [Event(SceneType.Current)]
    public class SceneChangeFinishEvent_CreateUIHelp : AEvent<Scene, SceneChangeFinish>
    {
        /// <summary>
        /// 当前项目不再在场景切换后自动打开旧帮助面板
        /// </summary>
        /// <param name="scene">当前逻辑场景</param>
        /// <param name="args">场景切换完成事件</param>
        protected override async ETTask Run(Scene scene, SceneChangeFinish args)
        {
            await ETTask.CompletedTask;
        }
    }
}
