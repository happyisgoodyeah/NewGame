namespace ET.Client
{
    /// <summary>
    /// 在通用指针按住事件中推进 Puzzle 拖拽。
    /// </summary>
    [Event(SceneType.Current)]
    public class PointerHold_UpdatePuzzleDrag : AEvent<Scene, PointerHold>
    {
        /// <summary>
        /// 超过拖拽阈值后开始正式拖拽，并在拖拽过程中同步输入位置。
        /// </summary>
        protected override async ETTask Run(Scene scene, PointerHold args)
        {
            PuzzleInputStateComponent inputStateComponent = scene.GetComponent<PuzzleInputStateComponent>();
            Puzzle draggingPuzzle = inputStateComponent?.GetDraggingPuzzle();
            if (draggingPuzzle != null)
            {
                PuzzleDragComponent draggingComponent = draggingPuzzle.GetComponent<PuzzleDragComponent>();
                if (draggingComponent == null)
                {
                    inputStateComponent.ClearDraggingPuzzle();
                    await ETTask.CompletedTask;
                    return;
                }

                draggingComponent.UpdateDrag(args.Context.WorldPosition);
                await ETTask.CompletedTask;
                return;
            }

            Puzzle pendingPuzzle = inputStateComponent?.GetPendingPressPuzzle();
            if (pendingPuzzle == null || !inputStateComponent.HasExceededDragThreshold(args.Context.ScreenPosition))
            {
                await ETTask.CompletedTask;
                return;
            }

            PuzzleDragComponent dragComponent = pendingPuzzle.GetComponent<PuzzleDragComponent>();
            if (dragComponent == null)
            {
                inputStateComponent.ClearPendingPress();
                await ETTask.CompletedTask;
                return;
            }

            dragComponent.BeginDrag(args.Context);
            inputStateComponent.SetDraggingPuzzle(pendingPuzzle);
            inputStateComponent.ClearPendingPress();
            await ETTask.CompletedTask;
        }
    }
}
