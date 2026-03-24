namespace ET.Client
{
    /// <summary>
    /// 在通用指针松开事件中结束 Puzzle 拖拽。
    /// </summary>
    [Event(SceneType.Current)]
    public class PointerUp_EndPuzzleDrag : AEvent<Scene, PointerUp>
    {
        /// <summary>
        /// 结束当前正在执行的 Puzzle 拖拽，并清空场景拖拽记录。
        /// </summary>
        protected override async ETTask Run(Scene scene, PointerUp args)
        {
            PuzzleInputStateComponent inputStateComponent = scene.GetComponent<PuzzleInputStateComponent>();
            Puzzle draggingPuzzle = inputStateComponent?.GetDraggingPuzzle();
            if (draggingPuzzle == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            PuzzleDragComponent dragComponent = draggingPuzzle.GetComponent<PuzzleDragComponent>();
            if (dragComponent != null)
            {
                Grid grid = scene.GetGrid();
                if (dragComponent.IsInGridSnapMode())
                {
                    dragComponent.FinalizeGridSnap();
                }
                else
                {
                    PuzzleInputResolveResult resolveResult = PuzzleInputResolver.Resolve(args.Context);
                    if (resolveResult.GridSlot == null)
                    {
                        dragComponent.RestoreInitialPosition();
                    }
                    else
                    {
                        bool placed = grid != null && draggingPuzzle.TryPlaceOnGrid(grid, resolveResult.GridSlot);
                        if (placed)
                        {
                            dragComponent.SnapToAnchor(grid);
                        }
                        else
                        {
                            if (grid != null && dragComponent.DragStartState == PuzzleState.Placed)
                            {
                                draggingPuzzle.TryPlaceOnGrid(grid, dragComponent.DragStartAnchorX, dragComponent.DragStartAnchorY);
                            }

                            dragComponent.RestoreDragStart();
                        }
                    }
                }
            }

            inputStateComponent.ClearDraggingPuzzle();
            await ETTask.CompletedTask;
        }
    }
}
