using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 关卡选择视图
    /// </summary>
    public partial class SelectLevelViewComponent : Entity, IDynamicEvent<SelectLevelView_LevelSlotGoGrid>
    {
        /// <summary>
        /// 每页显示关卡数量
        /// </summary>
        public const int PageSize = 12;

        /// <summary>
        /// 当前页码
        /// </summary>
        public int NowPage;

        /// <summary>
        /// 最大页码
        /// </summary>
        public int MaxPage;

        /// <summary>
        /// 已创建的关卡格组件
        /// </summary>
        public List<EntityRef<LevelSlotComponent>> LevelSlotComponents;
    }
}
