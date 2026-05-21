using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 成就主视图逻辑
    /// </summary>
    [FriendOf(typeof(MainArchiveViewComponent))]
    public static partial class MainArchiveViewComponentSystem
    {
        /// <summary>
        /// 初始化成就主视图
        /// </summary>
        /// <param name="self">成就主视图</param>
        [EntitySystem]
        private static void YIUIInitialize(this MainArchiveViewComponent self)
        {
        }

        /// <summary>
        /// 销毁成就主视图
        /// </summary>
        /// <param name="self">成就主视图</param>
        [EntitySystem]
        private static void Destroy(this MainArchiveViewComponent self)
        {
        }

        /// <summary>
        /// 打开成就主视图并刷新已完成关卡列表
        /// </summary>
        /// <param name="self">成就主视图</param>
        /// <returns>是否打开成功</returns>
        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this MainArchiveViewComponent self)
        {
            await GameplaySaveHelper.EnsureGameplaySaveAsync(self.Root().GetComponent<SaveManagerComponent>());
            self.RefreshArchiveShowList();
            await ETTask.CompletedTask;
            return true;
        }

        /// <summary>
        /// 将当前存档内已通关关卡同步到归档数据绑定
        /// </summary>
        /// <param name="self">成就主视图</param>
        public static void RefreshArchiveShowList(this MainArchiveViewComponent self)
        {
            LevelProgressSaveDataComponent progressData = GameplaySaveHelper.GetCurrentLevelProgress(self.Root().GetComponent<SaveManagerComponent>());
            List<int> passedLevelIds = progressData?.PassedLevelIds ?? new List<int>();
            self.u_DataArchiveShowList?.SetValue(passedLevelIds, true);
        }

        /// <summary>
        /// 关闭成就面板
        /// </summary>
        /// <param name="self">成就主视图</param>
        [YIUIInvoke(MainArchiveViewComponent.OnEventBackInvoke)]
        private static async ETTask OnEventBackInvoke(this MainArchiveViewComponent self)
        {
            await self.YIUIMgr().ClosePanelAsync<ArchivePanelComponent>();
        }
    }
}
