using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 在通用拖拽结束事件中结算 Puzzle 拖拽。
    /// </summary>
    [Event(SceneType.Current)]
    public class PointerDragEnd_EndPuzzleDrag : AEvent<Scene, PointerDragEnd>
    {
        /// <summary>
        /// 根据当前 Puzzle 交互状态完成拖拽落位或回退。
        /// </summary>
        protected override async ETTask Run(Scene scene, PointerDragEnd args)
        {
            DragInteractionStateComponent interactionStateComponent = scene.GetComponent<DragInteractionStateComponent>();
            Puzzle draggingPuzzle = interactionStateComponent?.GetDraggingTarget() as Puzzle;
            if (draggingPuzzle == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            this.HandleDragRelease(scene, draggingPuzzle, args.Context.WorldPosition);
            interactionStateComponent.ClearDraggingTarget();
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
    }
}
