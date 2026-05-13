namespace ET.Client
{
    /// <summary>
    /// Puzzle 数据实体创建完成后，为其创建对应的 PuzzleView。
    /// </summary>
    [Event(SceneType.Current)]
    public class AfterCreatePuzzle_CreatePuzzleView : AEvent<Scene, AfterCreatePuzzle>
    {
        /// <summary>
        /// 等待 Unity 场景和 GridView 可用后创建 PuzzleView。
        /// </summary>
        protected override async ETTask Run(Scene scene, AfterCreatePuzzle args)
        {
            Puzzle puzzle = args.Puzzle;
            await scene.WaitUntil(() => scene.IsDisposed || puzzle == null || puzzle.IsDisposed || scene.IsPuzzleViewReady() && scene.GetGrid()?.GetComponent<GridView>() != null);
            if (scene.IsDisposed || puzzle == null || puzzle.IsDisposed || puzzle.GetComponent<PuzzleView>() != null)
            {
                return;
            }

            Grid grid = scene.GetGrid();
            if (grid?.GetComponent<GridView>() == null)
            {
                return;
            }

            PuzzleConfig puzzleConfig = PuzzleCoreHelper.GetPuzzleConfig(puzzle.PuzzleConfigId);
            string assetLocation = ResourceLocationHelper.ToAssetLocation(puzzleConfig.PrefabPath);
            UnityEngine.GameObject prefab = await scene.GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<UnityEngine.GameObject>(assetLocation);
            if (scene.IsDisposed || puzzle.IsDisposed || puzzle.GetComponent<PuzzleView>() != null)
            {
                return;
            }

            UnityEngine.Vector3 worldPosition = new UnityEngine.Vector3(puzzle.InitialWorldPositionX, puzzle.InitialWorldPositionY, puzzle.InitialWorldPositionZ);
            puzzle.AddComponent<PuzzleView, UnityEngine.GameObject, UnityEngine.Vector3>(prefab, worldPosition);
            if (puzzle.GetComponent<PuzzleDragComponent>() == null)
            {
                puzzle.AddComponent<PuzzleDragComponent>();
            }
        }
    }
}
