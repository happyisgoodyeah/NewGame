using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 关卡格交互逻辑
    /// </summary>
    [FriendOf(typeof(LevelSlotComponent))]
    public static partial class LevelSlotComponentSystem
    {
        /// <summary>
        /// 初始化关卡格
        /// </summary>
        /// <param name="self">关卡格</param>
        [EntitySystem]
        private static void YIUIInitialize(this LevelSlotComponent self)
        {
        }

        /// <summary>
        /// 销毁关卡格
        /// </summary>
        /// <param name="self">关卡格</param>
        [EntitySystem]
        private static void Destroy(this LevelSlotComponent self)
        {
        }

        /// <summary>
        /// 设置关卡格显示状态
        /// </summary>
        /// <param name="self">关卡格</param>
        /// <param name="isUnlocked">是否解锁</param>
        /// <param name="isPassed">是否通关</param>
        /// <param name="gridConfigId">Grid 配置 id</param>
        /// <param name="displayIndex">显示序号</param>
        public static void SetSlotState(this LevelSlotComponent self, bool isUnlocked, bool isPassed, int gridConfigId, int displayIndex)
        {
            self.IsUnlocked = isUnlocked;
            self.IsPassed = isPassed;
            self.GridConfigId = gridConfigId;
            self.u_ComU_MaskRect.gameObject.SetActive(!isUnlocked);
            self.u_ComU_LevelSlotBtn.enabled = isUnlocked;
            self.u_ComU_LevelSlotBtn.interactable = isUnlocked;
            self.u_ComU_IsPassRect.gameObject.SetActive(isPassed);
            self.u_ComU_LevelCountText.SetText(displayIndex.ToString());
        }

        /// <summary>
        /// 进入当前关卡并切换到游戏视图
        /// </summary>
        /// <param name="self">关卡格</param>
        [YIUIInvoke(LevelSlotComponent.OnEventClickLevelInvoke)]
        private static async ETTask OnEventClickLevelInvoke(this LevelSlotComponent self)
        {
            if (!self.IsUnlocked)
            {
                await ETTask.CompletedTask;
                return;
            }

            Scene root = self.Root();
            await GameplaySaveHelper.SetCurrentLevelAsync(root.GetComponent<SaveManagerComponent>(), self.GridConfigId);
            Scene currentScene = await PuzzleLevelRuntimeHelper.EnsurePuzzleSceneAsync(root);
            if (currentScene == null || currentScene.IsDisposed)
            {
                return;
            }

            PuzzleLevelRuntimeHelper.StartLevel(currentScene, self.GridConfigId);
            await self.DynamicEvent(root, new SelectLevelView_LevelSlotGoGrid()
            {
                GridConfigId = self.GridConfigId,
            });
        }
    }
}
