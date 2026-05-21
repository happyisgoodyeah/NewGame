
namespace ET.Client
{
	[Event(SceneType.StateSync)]
	public class AppStartInitFinish_CreateLoginUI: AEvent<Scene, AppStartInitFinish>
	{
		/// <summary>
		/// 初始化 Gameplay 默认存档并打开主界面
		/// </summary>
		/// <param name="root">客户端根场景</param>
		/// <param name="args">应用启动初始化完成事件</param>
		protected override async ETTask Run(Scene root, AppStartInitFinish args)
		{
			await GameplaySaveHelper.EnsureGameplaySaveAsync(root.GetComponent<SaveManagerComponent>());
			await root.YIUIRoot().OpenPanelAsync<GameMainPanelPanelComponent>();
		}
	}
}
