namespace ET.Client
{
    /// <summary>
    /// 负责在当前阶段用硬编码数据快速搭建 PuzzleCore 运行时数据。
    /// </summary>
    public static class PuzzleBootstrapHelper
    {
        /// <summary>
        /// 在当前场景中创建默认 Grid 和其下所有 Slot。
        /// </summary>
        public static Grid CreateDefaultGrid(Scene scene)
        {
            SlotType[,] defaultGridLayout = CreateDefaultGridLayout();
            Grid existedGrid = scene.GetGrid();
            if (existedGrid != null)
            {
                return existedGrid;
            }

            int height = defaultGridLayout.GetLength(0);
            int width = defaultGridLayout.GetLength(1);

            Grid grid = scene.AddChildWithId<Grid, int, int>(PuzzleCoreConst.DefaultGridId, width, height);

            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    grid.AddSlot(x, y, defaultGridLayout[y, x]);
                }
            }

            grid.RefreshStatistics();

            Log.Info($"create default grid success: scene={scene.Name} size={width}x{height} placeable={grid.PlaceableCount} occupied={grid.OccupiedCount}");
            return grid;
        }

        /// <summary>
        /// 创建当前调试阶段使用的默认棋盘布局，第一维是行，第二维是列。
        /// </summary>
        private static SlotType[,] CreateDefaultGridLayout()
        {
            return new SlotType[,]
            {
                { SlotType.GridPlaceable, SlotType.GridPlaceable, SlotType.GridPlaceable, SlotType.GridPlaceable, SlotType.GridPlaceable },
                { SlotType.GridPlaceable, SlotType.GridBlocked,   SlotType.GridBlocked,   SlotType.GridPlaceable, SlotType.GridPlaceable },
                { SlotType.GridPlaceable, SlotType.GridPlaceable, SlotType.GridPlaceable, SlotType.GridPlaceable, SlotType.GridPlaceable },
                { SlotType.GridPlaceable, SlotType.GridPlaceable, SlotType.GridBlocked,   SlotType.GridBlocked,   SlotType.GridPlaceable },
                { SlotType.GridPlaceable, SlotType.GridPlaceable, SlotType.GridPlaceable, SlotType.GridPlaceable, SlotType.GridPlaceable },
            };
        }
    }
}
