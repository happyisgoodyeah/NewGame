using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 关卡选择视图逻辑
    /// </summary>
    [FriendOf(typeof(SelectLevelViewComponent))]
    public static partial class SelectLevelViewComponentSystem
    {
        /// <summary>
        /// 处理关卡格进入游戏视图的动态事件
        /// </summary>
        /// <param name="self">关卡选择视图</param>
        /// <param name="dynamicEvent">进入关卡事件</param>
        [EntitySystem]
        private static async ETTask DynamicEvent(this SelectLevelViewComponent self, SelectLevelView_LevelSlotGoGrid dynamicEvent)
        {
            await self.YIUIRoot().OpenPanelAsync<GameMainPanelPanelComponent, EGameMainPanelPanelViewEnum>(EGameMainPanelPanelViewEnum.GameView);
            await ETTask.CompletedTask;
        }

        /// <summary>
        /// 初始化关卡选择分页状态
        /// </summary>
        /// <param name="self">关卡选择视图</param>
        [EntitySystem]
        private static void YIUIInitialize(this SelectLevelViewComponent self)
        {
            self.NowPage = 1;
            self.MaxPage = GetMaxPage();
            self.LevelSlotComponents = new List<EntityRef<LevelSlotComponent>>();
        }

        /// <summary>
        /// 销毁关卡选择视图
        /// </summary>
        /// <param name="self">关卡选择视图</param>
        [EntitySystem]
        private static void Destroy(this SelectLevelViewComponent self)
        {
        }

        /// <summary>
        /// 打开关卡选择视图并刷新关卡状态
        /// </summary>
        /// <param name="self">关卡选择视图</param>
        /// <returns>是否打开成功</returns>
        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this SelectLevelViewComponent self)
        {
            await GameplaySaveHelper.EnsureGameplaySaveAsync(self.Root().GetComponent<SaveManagerComponent>());
            self.MaxPage = GetMaxPage();
            self.NowPage = Math.Max(1, Math.Min(self.NowPage, self.MaxPage));
            self.Refresh();
            return true;
        }

        /// <summary>
        /// 刷新关卡选择视图
        /// </summary>
        /// <param name="self">关卡选择视图</param>
        public static void Refresh(this SelectLevelViewComponent self)
        {
            self.RefreshButtons();
            self.u_ComU_TitleText.SetText($"第{self.NowPage}章");
            self.RefreshSlots();
        }

        /// <summary>
        /// 刷新翻页按钮显示状态
        /// </summary>
        /// <param name="self">关卡选择视图</param>
        public static void RefreshButtons(this SelectLevelViewComponent self)
        {
            self.u_ComU_LeftBtn.gameObject.SetActive(self.NowPage > 1);
            self.u_ComU_RightBtn.gameObject.SetActive(self.NowPage < self.MaxPage);
        }

        /// <summary>
        /// 刷新当前页的关卡格
        /// </summary>
        /// <param name="self">关卡选择视图</param>
        public static void RefreshSlots(this SelectLevelViewComponent self)
        {
            LevelProgressSaveDataComponent progressData = GameplaySaveHelper.GetCurrentLevelProgress(self.Root().GetComponent<SaveManagerComponent>());
            int startIndex = SelectLevelViewComponent.PageSize * (self.NowPage - 1);
            List<GridConfig> gridConfigs = GridConfigCategory.Instance.DataList;

            foreach (EntityRef<LevelSlotComponent> slot in self.LevelSlotComponents)
            {
                slot.Entity?.UIBase.OwnerGameObject.SetActive(false);
            }

            for (int i = 0; i < SelectLevelViewComponent.PageSize; ++i)
            {
                int configIndex = startIndex + i;
                if (configIndex >= gridConfigs.Count)
                {
                    return;
                }

                GridConfig gridConfig = gridConfigs[configIndex];
                LevelSlotComponent levelSlotComponent = GetOrCreateSlot(self, i);
                bool unlocked = progressData?.IsLevelUnlocked(gridConfig.Id) ?? configIndex == 0;
                bool passed = progressData?.IsLevelPassed(gridConfig.Id) ?? false;
                levelSlotComponent.UIBase.OwnerGameObject.SetActive(true);
                levelSlotComponent.SetSlotState(unlocked, passed, gridConfig.Id, configIndex + 1);
            }
        }

        /// <summary>
        /// 获取指定索引上的关卡格，没有则创建
        /// </summary>
        /// <param name="self">关卡选择视图</param>
        /// <param name="index">关卡格索引</param>
        /// <returns>关卡格组件</returns>
        private static LevelSlotComponent GetOrCreateSlot(SelectLevelViewComponent self, int index)
        {
            if (index < self.LevelSlotComponents.Count)
            {
                return self.LevelSlotComponents[index];
            }

            LevelSlotComponent levelSlotComponent = YIUIFactory.Instantiate<LevelSlotComponent>(self.Scene(), self, self.u_ComU_LevelContent);
            self.LevelSlotComponents.Add(levelSlotComponent);
            return levelSlotComponent;
        }

        /// <summary>
        /// 计算关卡选择最大页码
        /// </summary>
        /// <returns>最大页码</returns>
        private static int GetMaxPage()
        {
            int count = GridConfigCategory.Instance.DataList.Count;
            return Math.Max(1, (count + SelectLevelViewComponent.PageSize - 1) / SelectLevelViewComponent.PageSize);
        }

        /// <summary>
        /// 切换到上一页关卡
        /// </summary>
        /// <param name="self">关卡选择视图</param>
        [YIUIInvoke(SelectLevelViewComponent.OnEventU_LeftBtnClickInvoke)]
        private static void OnEventU_LeftBtnClickInvoke(this SelectLevelViewComponent self)
        {
            self.NowPage = Math.Max(1, self.NowPage - 1);
            self.Refresh();
        }

        /// <summary>
        /// 返回主菜单视图
        /// </summary>
        /// <param name="self">关卡选择视图</param>
        [YIUIInvoke(SelectLevelViewComponent.OnEventBackMenuBtnClickInvoke)]
        private static async ETTask OnEventBackMenuBtnClickInvoke(this SelectLevelViewComponent self)
        {
            await self.YIUIRoot().OpenPanelAsync<GameMainPanelPanelComponent, EGameMainPanelPanelViewEnum>(EGameMainPanelPanelViewEnum.Main1View);
        }

        /// <summary>
        /// 切换到下一页关卡
        /// </summary>
        /// <param name="self">关卡选择视图</param>
        [YIUIInvoke(SelectLevelViewComponent.OnEventU_RightBtnClickInvoke)]
        private static void OnEventU_RightBtnClickInvoke(this SelectLevelViewComponent self)
        {
            self.NowPage = Math.Min(self.MaxPage, self.NowPage + 1);
            self.Refresh();
        }
    }
}
