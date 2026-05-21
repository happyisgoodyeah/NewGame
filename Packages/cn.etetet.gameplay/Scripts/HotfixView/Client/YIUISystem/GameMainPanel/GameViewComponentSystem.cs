using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 关卡内视图逻辑
    /// </summary>
    [FriendOf(typeof(GameViewComponent))]
    public static partial class GameViewComponentSystem
    {
        /// <summary>
        /// 初始化关卡内视图
        /// </summary>
        /// <param name="self">关卡内视图</param>
        [EntitySystem]
        private static void YIUIInitialize(this GameViewComponent self)
        {
        }

        /// <summary>
        /// 销毁关卡内视图
        /// </summary>
        /// <param name="self">关卡内视图</param>
        [EntitySystem]
        private static void Destroy(this GameViewComponent self)
        {
        }

        /// <summary>
        /// 打开关卡内视图
        /// </summary>
        /// <param name="self">关卡内视图</param>
        /// <returns>是否打开成功</returns>
        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this GameViewComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        /// <summary>
        /// 放弃当前关卡并返回关卡选择
        /// </summary>
        /// <param name="self">关卡内视图</param>
        [YIUIInvoke(GameViewComponent.OnEventBackBtnClickInvoke)]
        private static async ETTask OnEventBackBtnClickInvoke(this GameViewComponent self)
        {
            PuzzleLevelRuntimeHelper.ClearCurrentLevel(self.Root().CurrentScene());
            await self.YIUIRoot().OpenPanelAsync<GameMainPanelPanelComponent, EGameMainPanelPanelViewEnum>(EGameMainPanelPanelViewEnum.SelectLevelView);
        }
    }
}
