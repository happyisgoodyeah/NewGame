using UnityEngine;

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
            self.PendingPressPuzzle = default;
            self.PendingPressScreenPosition = Vector2.zero;
            self.PendingPressWorldPosition = Vector3.zero;
        }

        /// <summary>
        /// 清理当前场景的 Puzzle 输入状态。
        /// </summary>
        [EntitySystem]
        private static void Destroy(this PuzzleInputStateComponent self)
        {
            self.DraggingPuzzle = default;
            self.PendingPressPuzzle = default;
            self.PendingPressScreenPosition = Vector2.zero;
            self.PendingPressWorldPosition = Vector3.zero;
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

        /// <summary>
        /// 记录一次新的待确认按压。
        /// </summary>
        /// <param name="self">当前场景的 Puzzle 输入状态组件。</param>
        /// <param name="puzzle">本次按下命中的 Puzzle。</param>
        /// <param name="screenPosition">按下时的屏幕坐标。</param>
        /// <param name="worldPosition">按下时的世界坐标。</param>
        public static void SetPendingPress(this PuzzleInputStateComponent self, Puzzle puzzle, Vector2 screenPosition, Vector3 worldPosition)
        {
            self.PendingPressPuzzle = puzzle;
            self.PendingPressScreenPosition = screenPosition;
            self.PendingPressWorldPosition = worldPosition;
        }

        /// <summary>
        /// 获取当前待确认按压对应的 Puzzle。
        /// </summary>
        /// <param name="self">当前场景的 Puzzle 输入状态组件。</param>
        /// <returns>当前待确认按压对应的 Puzzle；若不存在则返回空。</returns>
        public static Puzzle GetPendingPressPuzzle(this PuzzleInputStateComponent self)
        {
            return self.PendingPressPuzzle;
        }

        /// <summary>
        /// 判断当前待确认按压是否已超过正式拖拽阈值。
        /// </summary>
        /// <param name="self">当前场景的 Puzzle 输入状态组件。</param>
        /// <param name="screenPosition">当前鼠标屏幕坐标。</param>
        /// <returns>当前是否应进入正式拖拽。</returns>
        public static bool HasExceededDragThreshold(this PuzzleInputStateComponent self, Vector2 screenPosition)
        {
            return Vector2.Distance(self.PendingPressScreenPosition, screenPosition) > PuzzleViewConst.DragStartScreenThreshold;
        }

        /// <summary>
        /// 清空当前待确认按压记录。
        /// </summary>
        /// <param name="self">当前场景的 Puzzle 输入状态组件。</param>
        public static void ClearPendingPress(this PuzzleInputStateComponent self)
        {
            self.PendingPressPuzzle = default;
            self.PendingPressScreenPosition = Vector2.zero;
            self.PendingPressWorldPosition = Vector3.zero;
        }
    }
}
