namespace ET.Client
{
    /// <summary>
    /// 在通用指针按下事件中记录 Puzzle 的待确认按压状态。
    /// </summary>
    [Event(SceneType.Current)]
    public class PointerDown_BeginPuzzleDrag : AEvent<Scene, PointerDown>
    {
        /// <summary>
        /// 从通用输入命中结果中解析 Puzzle，并在命中时进入拖拽待确认状态。
        /// </summary>
        protected override async ETTask Run(Scene scene, PointerDown args)
        {
            PuzzleInputStateComponent inputStateComponent = scene.EnsurePuzzleInputState();
            if (inputStateComponent.GetDraggingPuzzle() != null)
            {
                await ETTask.CompletedTask;
                return;
            }

            inputStateComponent.ClearPendingPress();
            PuzzleInputResolveResult resolveResult = PuzzleInputResolver.Resolve(args.Context);
            Puzzle puzzle = resolveResult.Puzzle;
            if (puzzle == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            inputStateComponent.SetPendingPress(puzzle, args.Context.ScreenPosition, args.Context.WorldPosition);
            await ETTask.CompletedTask;
        }
    }
}
