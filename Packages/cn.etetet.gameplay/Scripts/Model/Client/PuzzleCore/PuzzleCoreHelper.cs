namespace ET.Client
{
    /// <summary>
    /// PuzzleCore 配置访问和运行时映射辅助方法。
    /// </summary>
    public static class PuzzleCoreHelper
    {
        /// <summary>
        /// 获取默认调试关卡使用的 Grid 配置。
        /// </summary>
        /// <returns>当前默认关卡对应的 Grid 配置。</returns>
        public static GridConfig GetDefaultGridConfig()
        {
            return GetGridConfig(PuzzleCoreConst.DefaultGridConfigId);
        }

        /// <summary>
        /// 按 id 获取 Grid 配置。
        /// </summary>
        /// <param name="gridConfigId">目标 Grid 配置 id。</param>
        /// <returns>目标 Grid 配置。</returns>
        public static GridConfig GetGridConfig(int gridConfigId)
        {
            GridConfig gridConfig = GridConfigCategory.Instance.Get(gridConfigId);
            if (gridConfig == null)
            {
                throw new System.Exception($"grid config not found: {gridConfigId}");
            }

            return gridConfig;
        }

        /// <summary>
        /// 按 id 获取 Puzzle 配置。
        /// </summary>
        /// <param name="puzzleConfigId">目标 Puzzle 配置 id。</param>
        /// <returns>目标 Puzzle 配置。</returns>
        public static PuzzleConfig GetPuzzleConfig(int puzzleConfigId)
        {
            PuzzleConfig puzzleConfig = PuzzleConfigCategory.Instance.Get(puzzleConfigId);
            if (puzzleConfig == null)
            {
                throw new System.Exception($"puzzle config not found: {puzzleConfigId}");
            }

            return puzzleConfig;
        }

        /// <summary>
        /// 按 id 获取 Slot 配置。
        /// </summary>
        /// <param name="slotConfigId">目标 Slot 配置 id。</param>
        /// <returns>目标 Slot 配置。</returns>
        public static SlotConfig GetSlotConfig(int slotConfigId)
        {
            SlotConfig slotConfig = SlotConfigCategory.Instance.Get(slotConfigId);
            if (slotConfig == null)
            {
                throw new System.Exception($"slot config not found: {slotConfigId}");
            }

            return slotConfig;
        }

        /// <summary>
        /// 将 Slot 配置映射为运行时 Grid Slot 类型。
        /// </summary>
        /// <param name="slotConfigId">目标 Slot 配置 id。</param>
        /// <returns>当前 Slot 在运行时使用的类型。</returns>
        public static SlotType ResolveGridSlotType(int slotConfigId)
        {
            SlotConfig slotConfig = GetSlotConfig(slotConfigId);
            return slotConfig.AllowPlace ? SlotType.GridPlaceable : SlotType.GridBlocked;
        }

    }
}
