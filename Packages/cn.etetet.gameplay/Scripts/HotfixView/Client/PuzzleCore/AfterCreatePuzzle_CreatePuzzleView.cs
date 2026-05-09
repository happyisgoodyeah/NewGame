using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// Puzzle 数据创建完成后，根据数据生成 PuzzleView。
    /// </summary>
    [Event(SceneType.Current)]
    public class AfterCreatePuzzle_CreatePuzzleView : AEvent<Scene, AfterCreatePuzzle>
    {
        /// <summary>
        /// 根据 Puzzle 配置和实例数据创建对应的 PuzzleView。
        /// </summary>
        protected override async ETTask Run(Scene scene, AfterCreatePuzzle args)
        {
            if (!PuzzleViewEventHelper.IsSceneReady(scene))
            {
                await ETTask.CompletedTask;
                return;
            }

            Puzzle puzzle = args.Puzzle;
            if (puzzle.GetComponent<PuzzleView>() != null)
            {
                await ETTask.CompletedTask;
                return;
            }

            Grid grid = scene.GetGrid();
            if (grid == null)
            {
                throw new UnityException("grid must exist before puzzle view is created");
            }

            PuzzleConfig puzzleConfig = PuzzleConfigHelper.GetPuzzleConfig(puzzle.PuzzleConfigId);
            string assetLocation = PuzzleAssetPathHelper.ToAssetLocation(puzzleConfig.PrefabPath);
            Vector3 worldPosition = PuzzleViewLayoutHelper.GetPuzzleWorldPosition(puzzle);
            puzzle.AddComponent<PuzzleView, string, Vector3>(assetLocation, worldPosition);
            if (puzzle.GetComponent<PuzzleDragComponent>() == null)
            {
                puzzle.AddComponent<PuzzleDragComponent>();
            }

            await ETTask.CompletedTask;
        }
    }
}
