namespace ET.Client
{
    /// <summary>
    /// 在通用指针按下事件中尝试开始 Puzzle 拖拽。
    /// </summary>
    [Event(SceneType.Current)]
    public class PointerDown_BeginPuzzleDrag : AEvent<Scene, PointerDown>
    {
        /// <summary>
        /// 从通用输入命中结果中解析 Puzzle，并在命中时开始拖拽。
        /// </summary>
        protected override async ETTask Run(Scene scene, PointerDown args)
        {
            PuzzleInputStateComponent inputStateComponent = scene.EnsurePuzzleInputState();
            if (inputStateComponent.GetDraggingPuzzle() != null)
            {
                await ETTask.CompletedTask;
                return;
            }

            PuzzleInputResolveResult resolveResult = PuzzleInputResolver.Resolve(args.Context);
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
            inputStateComponent.SetDraggingPuzzle(puzzle);
            await ETTask.CompletedTask;
        }
    }
}
