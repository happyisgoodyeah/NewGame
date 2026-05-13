namespace ET.Client
{
    /// <summary>
    /// 负责根据配表搭建 PuzzleCore 运行时数据。
    /// </summary>
    public static class PuzzleBootstrapHelper
    {
        /// <summary>
        /// 在当前场景中创建默认 Grid、Grid Slot 和 Puzzle 数据。
        /// </summary>
        public static Grid CreateDefaultGrid(Scene scene)
        {
            Grid existedGrid = scene.GetGrid();
            if (existedGrid != null)
            {
                return existedGrid;
            }

            GridConfig gridConfig = PuzzleCoreHelper.GetDefaultGridConfig();
            int width = gridConfig.X;
            int height = gridConfig.Y;
            ValidateGridConfig(gridConfig, width, height);

            Grid grid = scene.AddChildWithId<Grid, int, int>(PuzzleCoreConst.DefaultGridId, width, height);
            grid.GridConfigId = gridConfig.Id;
            EventSystem.Instance.Publish(scene, new AfterCreateGrid() { Grid = grid });

            CreateGridSlots(grid, gridConfig, width, height);
            CreatePuzzles(scene, gridConfig);
            grid.RefreshStatistics();

            Log.Info($"create default grid success: scene={scene.Name} gridConfigId={gridConfig.Id} size={width}x{height} placeable={grid.PlaceableCount} occupied={grid.OccupiedCount} puzzleCount={gridConfig.PuzzleList.Count}");
            return grid;
        }

        /// <summary>
        /// 根据 Grid 配置批量创建棋盘格。
        /// </summary>
        /// <param name="grid">目标棋盘。</param>
        /// <param name="gridConfig">当前关卡配置。</param>
        /// <param name="width">棋盘列数。</param>
        /// <param name="height">棋盘行数。</param>
        private static void CreateGridSlots(Grid grid, GridConfig gridConfig, int width, int height)
        {
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    int slotConfigId = gridConfig.SlotList[y * width + x];
                    grid.AddSlot(x, y, slotConfigId);
                }
            }
        }

        /// <summary>
        /// 根据 Grid 配置批量创建当前关卡内的全部 Puzzle。
        /// </summary>
        /// <param name="scene">当前场景。</param>
        /// <param name="gridConfig">当前关卡配置。</param>
        private static void CreatePuzzles(Scene scene, GridConfig gridConfig)
        {
            for (int index = 0; index < gridConfig.PuzzleList.Count; ++index)
            {
                puzzle puzzleInfo = gridConfig.PuzzleList[index];
                long puzzleEntityId = PuzzleCoreConst.DefaultPuzzleEntityIdStart + index + 1;
                CreatePuzzle(scene, puzzleEntityId, puzzleInfo);
            }
        }

        /// <summary>
        /// 根据 Puzzle 配置创建一块运行时 Puzzle 和其形状格。
        /// </summary>
        /// <param name="scene">当前场景。</param>
        /// <param name="puzzleId">目标 Puzzle 实体 id。</param>
        /// <param name="puzzleInfo">当前关卡内的 Puzzle 实例配置。</param>
        /// <returns>创建完成的 Puzzle。</returns>
        private static Puzzle CreatePuzzle(Scene scene, long puzzleId, puzzle puzzleInfo)
        {
            Puzzle existedPuzzle = scene.GetPuzzle(puzzleId);
            if (existedPuzzle != null)
            {
                return existedPuzzle;
            }

            PuzzleConfig puzzleConfig = PuzzleCoreHelper.GetPuzzleConfig(puzzleInfo.Id);
            Puzzle puzzleEntity = scene.AddChildWithId<Puzzle, int>(puzzleId, puzzleConfig.Id);
            puzzleEntity.InitialWorldPositionX = puzzleInfo.Trans.X;
            puzzleEntity.InitialWorldPositionY = puzzleInfo.Trans.Y;
            puzzleEntity.InitialWorldPositionZ = puzzleInfo.Trans.Z;

            foreach (System.Collections.Generic.List<int> slotOffset in puzzleConfig.SlotOffset)
            {
                ValidatePuzzleSlotOffset(puzzleConfig, slotOffset);
                puzzleEntity.AddSlot(slotOffset[0], slotOffset[1]);
            }

            EventSystem.Instance.Publish(scene, new AfterCreatePuzzle() { Puzzle = puzzleEntity });
            return puzzleEntity;
        }

        /// <summary>
        /// 校验当前 Grid 配置和尺寸定义是否一致。
        /// </summary>
        /// <param name="gridConfig">要校验的 Grid 配置。</param>
        /// <param name="width">当前采用的棋盘列数。</param>
        /// <param name="height">当前采用的棋盘行数。</param>
        private static void ValidateGridConfig(GridConfig gridConfig, int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                throw new System.Exception($"grid config size is invalid: id={gridConfig.Id} size={width}x{height}");
            }

            if (gridConfig.SlotList == null || gridConfig.SlotList.Count != width * height)
            {
                throw new System.Exception($"grid config slot list count mismatch: id={gridConfig.Id} expected={width * height} actual={gridConfig.SlotList?.Count ?? 0}");
            }

            if (gridConfig.PuzzleList == null)
            {
                throw new System.Exception($"grid config puzzle list is null: id={gridConfig.Id}");
            }
        }

        /// <summary>
        /// 校验单个 Puzzle 格偏移数据是否合法。
        /// </summary>
        /// <param name="puzzleConfig">所属 Puzzle 配置。</param>
        /// <param name="slotOffset">当前格子的偏移定义。</param>
        private static void ValidatePuzzleSlotOffset(PuzzleConfig puzzleConfig, System.Collections.Generic.List<int> slotOffset)
        {
            if (slotOffset == null || slotOffset.Count != 2)
            {
                throw new System.Exception($"puzzle config slot offset is invalid: id={puzzleConfig.Id}");
            }
        }
    }
}
