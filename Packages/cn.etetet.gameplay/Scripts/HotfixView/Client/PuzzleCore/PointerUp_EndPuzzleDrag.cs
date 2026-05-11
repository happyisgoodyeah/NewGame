using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 在通用指针松开事件中结束 Puzzle 拖拽或处理一次点击旋转。
    /// </summary>
    [Event(SceneType.Current)]
    public class PointerUp_EndPuzzleDrag : AEvent<Scene, PointerUp>
    {
        /// <summary>
        /// 根据当前输入状态完成拖拽结算，或将短按识别为一次 Puzzle 旋转。
        /// </summary>
        protected override async ETTask Run(Scene scene, PointerUp args)
        {
            PuzzleInputStateComponent inputStateComponent = scene.GetComponent<PuzzleInputStateComponent>();
            Puzzle draggingPuzzle = inputStateComponent?.GetDraggingPuzzle();
            if (draggingPuzzle != null)
            {
                this.HandleDragRelease(scene, draggingPuzzle, args.Context.WorldPosition);
                inputStateComponent.ClearDraggingPuzzle();
                inputStateComponent.ClearPendingPress();
                await ETTask.CompletedTask;
                return;
            }

            Puzzle pressedPuzzle = inputStateComponent?.GetPendingPressPuzzle();
            if (pressedPuzzle != null)
            {
                if (!inputStateComponent.HasExceededDragThreshold(args.Context.ScreenPosition))
                {
                    this.TryRotatePuzzle(scene, pressedPuzzle);
                }

                inputStateComponent.ClearPendingPress();
            }

            await ETTask.CompletedTask;
        }

        /// <summary>
        /// 处理一次已经进入正式拖拽状态的松手结算。
        /// </summary>
        /// <param name="scene">当前场景。</param>
        /// <param name="draggingPuzzle">本次正在拖拽的 Puzzle。</param>
        /// <param name="pointerWorldPosition">松手时指针所在的世界坐标。</param>
        private void HandleDragRelease(Scene scene, Puzzle draggingPuzzle, Vector3 pointerWorldPosition)
        {
            PuzzleDragComponent dragComponent = draggingPuzzle.GetComponent<PuzzleDragComponent>();
            PuzzleView puzzleView = draggingPuzzle.GetComponent<PuzzleView>();
            Grid grid = scene.GetGrid();
            GridView gridView = grid?.GetComponent<GridView>();
            if (dragComponent == null || puzzleView == null)
            {
                return;
            }

            if (dragComponent.IsInGridSnapMode())
            {
                if (dragComponent.CanFinalizeGridSnap() && grid != null)
                {
                    dragComponent.SnapToAnchor(grid);
                }
                else
                {
                    dragComponent.RestoreInitialPosition();
                }

                return;
            }

            if (grid == null || gridView == null)
            {
                dragComponent.RestoreInitialPosition();
                return;
            }

            Vector2 gridCoordinate = gridView.WorldToGridCoordinate(pointerWorldPosition);
            int anchorX = Mathf.RoundToInt(gridCoordinate.x);
            int anchorY = Mathf.RoundToInt(gridCoordinate.y);
            bool placed = grid.Contains(anchorX, anchorY) && draggingPuzzle.TryPlaceOnGrid(grid, anchorX, anchorY);
            if (placed)
            {
                dragComponent.SnapToAnchor(grid);
                return;
            }

            dragComponent.RestoreInitialPosition();
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
