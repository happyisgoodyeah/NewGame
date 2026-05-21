using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 关卡选择格
    /// </summary>
    public partial class LevelSlotComponent : Entity
    {
        /// <summary>
        /// 当前格对应的 Grid 配置 id
        /// </summary>
        public int GridConfigId;

        /// <summary>
        /// 当前关卡是否已解锁
        /// </summary>
        public bool IsUnlocked;

        /// <summary>
        /// 当前关卡是否已通关
        /// </summary>
        public bool IsPassed;
    }
}
