namespace ET.Client
{
    /// <summary>
    /// 在通用指针按住事件中推进 Puzzle 拖拽。
    /// </summary>
    [Event(SceneType.Current)]
    public class PointerHold_UpdatePuzzleDrag : AEvent<Scene, PointerHold>
    {
        /// <summary>
        /// 将当前输入位置同步到正在拖拽的 Puzzle。
        /// </summary>
        protected override async ETTask Run(Scene scene, PointerHold args)
        {
            PuzzleInputStateComponent inputStateComponent = scene.GetComponent<PuzzleInputStateComponent>();
            Puzzle draggingPuzzle = inputStateComponent?.GetDraggingPuzzle();
            if (draggingPuzzle == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            PuzzleDragComponent dragComponent = draggingPuzzle.GetComponent<PuzzleDragComponent>();
            if (dragComponent == null)
            {
                inputStateComponent.ClearDraggingPuzzle();
                await ETTask.CompletedTask;
                return;
            }

            dragComponent.UpdateDrag(args.Context.WorldPosition);
            await ETTask.CompletedTask;
        }
    }
}
