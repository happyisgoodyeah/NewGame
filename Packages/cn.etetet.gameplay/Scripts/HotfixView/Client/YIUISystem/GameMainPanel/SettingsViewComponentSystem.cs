using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 设置视图逻辑
    /// </summary>
    [FriendOf(typeof(SettingsViewComponent))]
    public static partial class SettingsViewComponentSystem
    {
        /// <summary>
        /// 初始化设置视图
        /// </summary>
        /// <param name="self">设置视图</param>
        [EntitySystem]
        private static void YIUIInitialize(this SettingsViewComponent self)
        {
        }

        /// <summary>
        /// 销毁设置视图
        /// </summary>
        /// <param name="self">设置视图</param>
        [EntitySystem]
        private static void Destroy(this SettingsViewComponent self)
        {
        }

        /// <summary>
        /// 打开设置视图
        /// </summary>
        /// <param name="self">设置视图</param>
        /// <returns>是否打开成功</returns>
        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this SettingsViewComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        /// <summary>
        /// 返回主菜单视图
        /// </summary>
        /// <param name="self">设置视图</param>
        [YIUIInvoke(SettingsViewComponent.OnEventBackBtnClickInvoke)]
        private static async ETTask OnEventBackBtnClickInvoke(this SettingsViewComponent self)
        {
            await self.YIUIRoot().OpenPanelAsync<GameMainPanelPanelComponent, EGameMainPanelPanelViewEnum>(EGameMainPanelPanelViewEnum.Main1View);
        }
    }
}
