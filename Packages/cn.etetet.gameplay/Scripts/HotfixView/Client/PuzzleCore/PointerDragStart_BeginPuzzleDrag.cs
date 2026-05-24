namespace ET.Client
{
    /// <summary>
    /// 在通用拖拽开始事件中启动 Puzzle 拖拽
    /// </summary>
    [Event(SceneType.Current)]
    public class PointerDragStart_BeginPuzzleDrag : AEvent<Scene, PointerDragStart>
    {
        /// <summary>
        /// 从按压起始命中结果中解析 Puzzle，并在命中时启动对应拖拽组件
        /// </summary>
        protected override async ETTask Run(Scene scene, PointerDragStart args)
        {
            DragInteractionStateComponent interactionStateComponent = scene.GetOrAddDragInteractionState();
            if (interactionStateComponent.GetDraggingTarget() != null)
            {
                await ETTask.CompletedTask;
                return;
            }

            PuzzleInputResolveResult resolveResult = PuzzleInputResolver.ResolvePress(args.Context);
            Puzzle puzzle = resolveResult.Puzzle;
            if (puzzle == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            PuzzleDragComponent dragComponent = puzzle.GetComponent<PuzzleDragComponent>();
            if (dragComponent == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            dragComponent.BeginDrag(args.Context);
            interactionStateComponent.SetDraggingTarget(puzzle);
            await ETTask.CompletedTask;
        }
    }
}
