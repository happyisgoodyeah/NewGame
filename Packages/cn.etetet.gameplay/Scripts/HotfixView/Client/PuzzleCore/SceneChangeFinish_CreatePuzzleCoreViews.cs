namespace ET.Client
{
    /// <summary>
    /// 当前场景切换完成后，集中生成 PuzzleCore 的 Scene 表现层对象。
    /// </summary>
    [Event(SceneType.Current)]
    public class SceneChangeFinish_CreatePuzzleCoreViews : AEvent<Scene, SceneChangeFinish>
    {
        /// <summary>
        /// 在 Unity 场景加载完成后，按数据层结构一次性创建 GridView、PuzzleView 和 SlotView。
        /// </summary>
        protected override async ETTask Run(Scene scene, SceneChangeFinish args)
        {
            CreatePuzzleCoreViews(scene);
            await ETTask.CompletedTask;
        }

        /// <summary>
        /// 根据当前场景已经创建好的 PuzzleCore 数据补齐表现层；这里不再拆成多个创建事件，避免初始化链路过度分散。
        /// </summary>
        /// <param name="scene">当前 ET 逻辑场景。</param>
        private static void CreatePuzzleCoreViews(Scene scene)
        {
            if (!scene.TryGetUnityScene(out _))
            {
                return;
            }

            scene.EnsureInitialized();

            Grid grid = scene.GetGrid();
            if (grid != null)
            {
                CreateGridView(grid);
                CreateSlotViews(grid);
            }

            if (scene.ChildrenCount() == 0)
            {
                return;
            }

            foreach (Entity child in scene.Children.Values)
            {
                Puzzle puzzle = child as Puzzle;
                if (puzzle == null)
                {
                    continue;
                }

                CreatePuzzleView(scene, puzzle);
                CreateSlotViews(puzzle);
            }
        }

        /// <summary>
        /// 为 Grid 数据实体创建居中的 GridView；已存在时保持原对象不重复创建。
        /// </summary>
        /// <param name="grid">当前关卡棋盘数据。</param>
        private static void CreateGridView(Grid grid)
        {
            if (grid.GetComponent<GridView>() != null)
            {
                return;
            }

            UnityEngine.Vector3 worldPosition = PuzzleViewLayoutHelper.GetGridWorldPosition();
            grid.AddComponent<GridView, UnityEngine.Vector3, float>(worldPosition, PuzzleViewConst.GridCellSize);
        }

        /// <summary>
        /// 为 Puzzle 数据实体创建 PuzzleView 和拖拽组件；依赖 Grid 已经存在，保证坐标换算和吸附逻辑有基准。
        /// </summary>
        /// <param name="scene">当前 ET 逻辑场景。</param>
        /// <param name="puzzle">需要创建表现层的 Puzzle 数据。</param>
        private static void CreatePuzzleView(Scene scene, Puzzle puzzle)
        {
            if (puzzle.GetComponent<PuzzleView>() != null)
            {
                return;
            }

            Grid grid = scene.GetGrid();
            if (grid == null)
            {
                throw new UnityEngine.UnityException("grid must exist before puzzle view is created");
            }

            PuzzleConfig puzzleConfig = PuzzleConfigHelper.GetPuzzleConfig(puzzle.PuzzleConfigId);
            string assetLocation = PuzzleAssetPathHelper.ToAssetLocation(puzzleConfig.PrefabPath);
            UnityEngine.Vector3 worldPosition = PuzzleViewLayoutHelper.GetPuzzleWorldPosition(puzzle);
            puzzle.AddComponent<PuzzleView, string, UnityEngine.Vector3>(assetLocation, worldPosition);
            if (puzzle.GetComponent<PuzzleDragComponent>() == null)
            {
                puzzle.AddComponent<PuzzleDragComponent>();
            }
        }

        /// <summary>
        /// 为指定父实体下的所有 Slot 创建 SlotView；Grid Slot 会生成可见格子，Puzzle Slot 会绑定到 Puzzle prefab 内的节点。
        /// </summary>
        /// <param name="parent">Grid 或 Puzzle 数据实体。</param>
        private static void CreateSlotViews(Entity parent)
        {
            if (parent.ChildrenCount() == 0)
            {
                return;
            }

            foreach (Entity child in parent.Children.Values)
            {
                Slot slot = child as Slot;
                if (slot == null || slot.GetComponent<SlotView>() != null)
                {
                    continue;
                }

                slot.AddComponent<SlotView>();
            }
        }
    }
}
