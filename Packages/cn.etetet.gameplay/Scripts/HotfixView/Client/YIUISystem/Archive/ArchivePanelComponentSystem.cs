using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 成就面板逻辑
    /// </summary>
    [FriendOf(typeof(ArchivePanelComponent))]
    public static partial class ArchivePanelComponentSystem
    {
        /// <summary>
        /// 初始化成就面板并打开主视图
        /// </summary>
        /// <param name="self">成就面板</param>
        [EntitySystem]
        private static void YIUIInitialize(this ArchivePanelComponent self)
        {
            self.OpenMainViewAsync().NoContext();
        }

        /// <summary>
        /// 销毁成就面板
        /// </summary>
        /// <param name="self">成就面板</param>
        [EntitySystem]
        private static void Destroy(this ArchivePanelComponent self)
        {
        }

        /// <summary>
        /// 打开成就面板
        /// </summary>
        /// <param name="self">成就面板</param>
        /// <returns>是否打开成功</returns>
        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this ArchivePanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        /// <summary>
        /// 打开成就主视图
        /// </summary>
        /// <param name="self">成就面板</param>
        /// <returns>是否打开成功</returns>
        private static async ETTask<bool> OpenMainViewAsync(this ArchivePanelComponent self)
        {
            await self.UIPanel.OpenViewAsync<MainArchiveViewComponent>();
            return true;
        }
    }
}
