using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 主菜单按钮逻辑
    /// </summary>
    [FriendOf(typeof(Main1ViewComponent))]
    [FriendOf(typeof(GameMainPanelPanelComponent))]
    public static partial class Main1ViewComponentSystem
    {
        /// <summary>
        /// 初始化主菜单视图
        /// </summary>
        /// <param name="self">主菜单视图</param>
        [EntitySystem]
        private static void YIUIInitialize(this Main1ViewComponent self)
        {
        }

        /// <summary>
        /// 销毁主菜单视图
        /// </summary>
        /// <param name="self">主菜单视图</param>
        [EntitySystem]
        private static void Destroy(this Main1ViewComponent self)
        {
        }

        /// <summary>
        /// 打开主菜单视图
        /// </summary>
        /// <param name="self">主菜单视图</param>
        /// <returns>是否打开成功</returns>
        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this Main1ViewComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        /// <summary>
        /// 打开关卡选择视图
        /// </summary>
        /// <param name="self">主菜单视图</param>
        [YIUIInvoke(Main1ViewComponent.OnEventStartGameInvoke)]
        private static async ETTask OnEventStartGameInvoke(this Main1ViewComponent self)
        {
            await self.YIUIRoot().OpenPanelAsync<GameMainPanelPanelComponent, EGameMainPanelPanelViewEnum>(EGameMainPanelPanelViewEnum.SelectLevelView);
        }

        /// <summary>
        /// 打开设置视图
        /// </summary>
        /// <param name="self">主菜单视图</param>
        [YIUIInvoke(Main1ViewComponent.OnEventSettingsEnterInvoke)]
        private static async ETTask OnEventSettingsEnterInvoke(this Main1ViewComponent self)
        {
            await self.YIUIRoot().OpenPanelAsync<GameMainPanelPanelComponent, EGameMainPanelPanelViewEnum>(EGameMainPanelPanelViewEnum.SettingsView);
        }

        /// <summary>
        /// 打开图鉴面板
        /// </summary>
        /// <param name="self">主菜单视图</param>
        [YIUIInvoke(Main1ViewComponent.OnEventArchiveEnterInvoke)]
        private static async ETTask OnEventArchiveEnterInvoke(this Main1ViewComponent self)
        {
            await self.YIUIRoot().OpenPanelAsync<ArchivePanelComponent>();
        }

        /// <summary>
        /// 退出当前游戏运行
        /// </summary>
        /// <param name="self">主菜单视图</param>
        [YIUIInvoke(Main1ViewComponent.OnEventExitGameInvoke)]
        private static async ETTask OnEventExitGameInvoke(this Main1ViewComponent self)
        {
#if UNITY_EDITOR
            Log.Info("test");
            Log.Error("test");
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            await ETTask.CompletedTask;
        }
    }
}
