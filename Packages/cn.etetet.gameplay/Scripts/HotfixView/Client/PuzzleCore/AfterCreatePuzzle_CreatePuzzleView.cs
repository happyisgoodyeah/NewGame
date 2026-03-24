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
        /// 根据 Puzzle 数据把拼图摆放到 Grid 左右两侧的初始位置。
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

            Vector3 worldPosition = PuzzleViewLayoutHelper.GetPuzzleWorldPosition(grid, puzzle);
            puzzle.AddComponent<PuzzleView, string, Vector3>(PuzzleViewConst.DefaultPuzzlePrefabPath, worldPosition);
            if (puzzle.GetComponent<PuzzleDragComponent>() == null)
            {
                puzzle.AddComponent<PuzzleDragComponent>();
            }

            await ETTask.CompletedTask;
        }
    }
}
