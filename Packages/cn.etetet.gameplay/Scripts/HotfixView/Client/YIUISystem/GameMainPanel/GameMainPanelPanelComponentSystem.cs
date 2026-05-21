using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// GameMainPanel 面板切换逻辑
    /// </summary>
    [FriendOf(typeof(GameMainPanelPanelComponent))]
    public static partial class GameMainPanelPanelComponentSystem
    {
        /// <summary>
        /// 按外部传入枚举打开对应 View
        /// </summary>
        /// <param name="self">GameMainPanel 面板</param>
        /// <param name="viewEnum">目标 View 枚举</param>
        /// <returns>是否打开成功</returns>
        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this GameMainPanelPanelComponent self, EGameMainPanelPanelViewEnum viewEnum)
        {
            await self.UIPanel.OpenViewAsync(viewEnum.ToString());
            return true;
        }

        /// <summary>
        /// 初始化时默认打开主菜单
        /// </summary>
        /// <param name="self">GameMainPanel 面板</param>
        [EntitySystem]
        private static void YIUIInitialize(this GameMainPanelPanelComponent self)
        {
            self.UIPanel.OpenViewAsync<Main1ViewComponent>().NoContext();
        }

        /// <summary>
        /// 销毁面板时清理临时状态
        /// </summary>
        /// <param name="self">GameMainPanel 面板</param>
        [EntitySystem]
        private static void Destroy(this GameMainPanelPanelComponent self)
        {
        }

        /// <summary>
        /// 打开面板默认保持当前 View
        /// </summary>
        /// <param name="self">GameMainPanel 面板</param>
        /// <returns>是否打开成功</returns>
        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this GameMainPanelPanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }
    }
}
