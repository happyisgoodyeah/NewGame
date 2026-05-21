namespace ET.Client
{
    /// <summary>
    /// 关卡完成后保存进度并回到关卡选择
    /// </summary>
    [Event(SceneType.Current)]
    public class PuzzleLevelCompleted_SaveAndReturnSelect : AEvent<Scene, PuzzleLevelCompleted>
    {
        /// <summary>
        /// 写入通关状态、清理关卡数据并刷新关卡选择界面
        /// </summary>
        /// <param name="scene">当前关卡逻辑场景</param>
        /// <param name="args">关卡完成事件</param>
        protected override async ETTask Run(Scene scene, PuzzleLevelCompleted args)
        {
            SaveResult saveResult = await GameplaySaveHelper.PassLevelAsync(scene.Root().GetComponent<SaveManagerComponent>(), args.GridConfigId);
            if (saveResult != SaveResult.Success)
            {
                Log.Error($"保存关卡完成进度失败: {saveResult}");
            }

            PuzzleLevelRuntimeHelper.ClearCurrentLevel(scene);
            await scene.YIUIRoot().OpenPanelAsync<GameMainPanelPanelComponent, EGameMainPanelPanelViewEnum>(EGameMainPanelPanelViewEnum.SelectLevelView);
        }
    }
}
