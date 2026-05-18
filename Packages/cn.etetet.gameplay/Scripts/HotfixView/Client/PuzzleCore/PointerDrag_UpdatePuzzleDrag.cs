namespace ET.Client
{
    /// <summary>
    /// 在通用拖拽持续事件中推进 Puzzle 拖拽。
    /// </summary>
    [Event(SceneType.Current)]
    public class PointerDrag_UpdatePuzzleDrag : AEvent<Scene, PointerDrag>
    {
        /// <summary>
        /// 将当前拖拽指针位置同步给正在拖拽的 Puzzle。
        /// </summary>
        protected override async ETTask Run(Scene scene, PointerDrag args)
        {
            DragInteractionStateComponent interactionStateComponent = scene.GetComponent<DragInteractionStateComponent>();
            Puzzle draggingPuzzle = interactionStateComponent?.GetDraggingTarget() as Puzzle;
            if (draggingPuzzle == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            PuzzleDragComponent draggingComponent = draggingPuzzle.GetComponent<PuzzleDragComponent>();
            if (draggingComponent == null)
            {
                interactionStateComponent.ClearDraggingTarget();
                await ETTask.CompletedTask;
                return;
            }

            draggingComponent.UpdateDrag(args.Context.WorldPosition);
            await ETTask.CompletedTask;
        }
    }
}
