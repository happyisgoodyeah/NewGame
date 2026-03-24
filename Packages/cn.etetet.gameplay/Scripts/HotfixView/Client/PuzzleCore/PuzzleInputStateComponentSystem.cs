namespace ET.Client
{
    /// <summary>
    /// PuzzleInputStateComponent 的状态维护与快捷访问逻辑。
    /// </summary>
    [EntitySystemOf(typeof(PuzzleInputStateComponent))]
    public static partial class PuzzleInputStateComponentSystem
    {
        /// <summary>
        /// 初始化当前场景的 Puzzle 输入状态。
        /// </summary>
        [EntitySystem]
        private static void Awake(this PuzzleInputStateComponent self)
        {
            self.DraggingPuzzle = default;
        }

        /// <summary>
        /// 清理当前场景的 Puzzle 输入状态。
        /// </summary>
        [EntitySystem]
        private static void Destroy(this PuzzleInputStateComponent self)
        {
            self.DraggingPuzzle = default;
        }

        /// <summary>
        /// 确保当前场景已经挂载 Puzzle 输入状态组件。
        /// </summary>
        /// <param name="self">当前场景。</param>
        /// <returns>当前场景对应的 Puzzle 输入状态组件。</returns>
        public static PuzzleInputStateComponent EnsurePuzzleInputState(this Scene self)
        {
            PuzzleInputStateComponent inputStateComponent = self.GetComponent<PuzzleInputStateComponent>();
            if (inputStateComponent == null)
            {
                inputStateComponent = self.AddComponent<PuzzleInputStateComponent>();
            }

            return inputStateComponent;
        }

        /// <summary>
        /// 获取当前场景正在被拖拽的 Puzzle。
        /// </summary>
        /// <param name="self">当前场景的 Puzzle 输入状态组件。</param>
        /// <returns>当前正在被拖拽的 Puzzle；若不存在则返回空。</returns>
        public static Puzzle GetDraggingPuzzle(this PuzzleInputStateComponent self)
        {
            return self.DraggingPuzzle;
        }

        /// <summary>
        /// 记录当前场景正在被拖拽的 Puzzle。
        /// </summary>
        /// <param name="self">当前场景的 Puzzle 输入状态组件。</param>
        /// <param name="puzzle">要记录为正在拖拽的 Puzzle。</param>
        public static void SetDraggingPuzzle(this PuzzleInputStateComponent self, Puzzle puzzle)
        {
            self.DraggingPuzzle = puzzle;
        }

        /// <summary>
        /// 清空当前场景的拖拽记录。
        /// </summary>
        /// <param name="self">当前场景的 Puzzle 输入状态组件。</param>
        public static void ClearDraggingPuzzle(this PuzzleInputStateComponent self)
        {
            self.DraggingPuzzle = default;
        }
    }
}
