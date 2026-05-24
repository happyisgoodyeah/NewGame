using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 图鉴主视图逻辑
    /// </summary>
    [FriendOf(typeof(MainArchiveViewComponent))]
    public static partial class MainArchiveViewComponentSystem
    {
        /// <summary>
        /// 初始化图鉴主视图
        /// </summary>
        /// <param name="self">图鉴主视图</param>
        [EntitySystem]
        private static void YIUIInitialize(this MainArchiveViewComponent self)
        {
        }

        /// <summary>
        /// 销毁图鉴主视图
        /// </summary>
        /// <param name="self">图鉴主视图</param>
        [EntitySystem]
        private static void Destroy(this MainArchiveViewComponent self)
        {
        }

        /// <summary>
        /// 打开图鉴主视图并刷新已解锁拼图列表
        /// </summary>
        /// <param name="self">图鉴主视图</param>
        /// <returns>是否打开成功</returns>
        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this MainArchiveViewComponent self)
        {
            await GameplaySaveHelper.LoadGameplaySaveAsync(self.Root().GetComponent<SaveManagerComponent>());
            self.RefreshArchiveShowList();
            await ETTask.CompletedTask;
            return true;
        }

        /// <summary>
        /// 将当前存档内已解锁拼图同步到图鉴数据绑定
        /// </summary>
        /// <param name="self">图鉴主视图</param>
        public static void RefreshArchiveShowList(this MainArchiveViewComponent self)
        {
            PuzzleArchiveSaveDataComponent archiveData = GameplaySaveHelper.GetCurrentPuzzleArchive(self.Root().GetComponent<SaveManagerComponent>());
            List<int> unlockedPuzzleIds = archiveData?.UnlockedPuzzleIds ?? new List<int>();
            self.u_DataArchiveShowList?.SetValue(unlockedPuzzleIds, true);
        }

        /// <summary>
        /// 关闭图鉴面板
        /// </summary>
        /// <param name="self">图鉴主视图</param>
        [YIUIInvoke(MainArchiveViewComponent.OnEventBackInvoke)]
        private static async ETTask OnEventBackInvoke(this MainArchiveViewComponent self)
        {
            await self.YIUIMgr().ClosePanelAsync<ArchivePanelComponent>();
        }
    }
}
