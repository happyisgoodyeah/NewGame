namespace ET.Client
{
    /// <summary>
    /// 负责在当前阶段用硬编码数据快速搭建 PuzzleCore 运行时数据。
    /// </summary>
    public static class PuzzleBootstrapHelper
    {
        /// <summary>
        /// 在当前场景中创建默认 Grid，并在数据创建完成后通知表现层生成 GridView。
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
            EventSystem.Instance.Publish(scene, new AfterCreateGrid() { Grid = grid });

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
        /// 在当前场景中创建一个默认的 1x1 Puzzle 数据，并在数据创建完成后通知表现层生成 PuzzleView。
        /// </summary>
        public static Puzzle CreateDefaultPuzzle(Scene scene, long puzzleId)
        {
            Puzzle existedPuzzle = scene.GetPuzzle(puzzleId);
            if (existedPuzzle != null)
            {
                return existedPuzzle;
            }

            Puzzle puzzle = scene.AddChildWithId<Puzzle, int>(puzzleId, PuzzleCoreConst.DefaultPuzzleConfigId);
            EventSystem.Instance.Publish(scene, new AfterCreatePuzzle() { Puzzle = puzzle });
            puzzle.AddSlot(0, 0);
            return puzzle;
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
