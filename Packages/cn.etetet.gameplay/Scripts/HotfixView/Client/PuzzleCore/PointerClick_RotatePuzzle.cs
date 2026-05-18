namespace ET.Client
{
    /// <summary>
    /// 在通用点击事件中处理 Puzzle 旋转。
    /// </summary>
    [Event(SceneType.Current)]
    public class PointerClick_RotatePuzzle : AEvent<Scene, PointerClick>
    {
        /// <summary>
        /// 从按压起始命中结果中解析 Puzzle，并在点击命中时尝试旋转。
        /// </summary>
        protected override async ETTask Run(Scene scene, PointerClick args)
        {
            DragInteractionStateComponent interactionStateComponent = scene.GetComponent<DragInteractionStateComponent>();
            if (interactionStateComponent?.GetDraggingTarget() != null)
            {
                await ETTask.CompletedTask;
                return;
            }

            PuzzleInputResolveResult resolveResult = PuzzleInputResolver.ResolvePress(args.Context);
            Puzzle puzzle = resolveResult.Puzzle;
            if (puzzle != null)
            {
                this.TryRotatePuzzle(scene, puzzle);
            }

            await ETTask.CompletedTask;
        }

        /// <summary>
        /// 执行一次 Puzzle 点击旋转，并在已放置时校验旋转后的合法性。
        /// </summary>
        /// <param name="scene">当前场景。</param>
        /// <param name="puzzle">要旋转的 Puzzle。</param>
        private void TryRotatePuzzle(Scene scene, Puzzle puzzle)
        {
            PuzzleView puzzleView = puzzle.GetComponent<PuzzleView>();
            if (puzzleView == null)
            {
                return;
            }

            Grid grid = scene.GetGrid();
            PuzzleRotation previousRotation = puzzle.Rotation;
            PuzzleState previousState = puzzle.State;
            int previousAnchorX = puzzle.AnchorX;
            int previousAnchorY = puzzle.AnchorY;
            if (previousState == PuzzleState.Placed && grid != null)
            {
                puzzle.ClearPlacement(grid);
            }

            puzzle.RotateClockwise();
            bool rotationApplied = previousState != PuzzleState.Placed || grid == null || puzzle.TryPlaceOnGrid(grid, previousAnchorX, previousAnchorY);
            if (!rotationApplied)
            {
                puzzle.SetRotation(previousRotation);
                if (previousState == PuzzleState.Placed && grid != null)
                {
                    puzzle.TryPlaceOnGrid(grid, previousAnchorX, previousAnchorY);
                }

                puzzleView.PlayBlockedRotation();
                puzzleView.RestoreVisualPriority();
                return;
            }

            puzzleView.RefreshRotation(true);
            puzzleView.RestoreVisualPriority();
        }
    }
}
