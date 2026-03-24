namespace ET.Client
{
    /// <summary>
    /// PuzzleCore 对通用输入命中结果的业务解析结果。
    /// </summary>
    public struct PuzzleInputResolveResult
    {
        /// <summary>
        /// 当前命中的 PuzzleView。
        /// </summary>
        public PuzzleView PuzzleView;

        /// <summary>
        /// 当前命中的 Puzzle。
        /// </summary>
        public Puzzle Puzzle;

        /// <summary>
        /// 当前命中的 SlotView。
        /// </summary>
        public SlotView SlotView;

        /// <summary>
        /// 当前命中的 Slot。
        /// </summary>
        public Slot Slot;

        /// <summary>
        /// 当前命中的 Puzzle SlotView。
        /// </summary>
        public SlotView PuzzleSlotView;

        /// <summary>
        /// 当前命中的 Puzzle Slot。
        /// </summary>
        public Slot PuzzleSlot;

        /// <summary>
        /// 当前命中的 Grid SlotView。
        /// </summary>
        public SlotView GridSlotView;

        /// <summary>
        /// 当前命中的 Grid Slot。
        /// </summary>
        public Slot GridSlot;
    }

    /// <summary>
    /// PuzzleCore 输入解析辅助方法，负责从通用输入结果中筛选 PuzzleCore 相关对象。
    /// </summary>
    public static class PuzzleInputResolver
    {
        /// <summary>
        /// 从通用输入上下文中筛选首个命中的 PuzzleView 与 SlotView。
        /// </summary>
        /// <param name="pointerContext">输入层发布的通用输入上下文。</param>
        /// <returns>当前命中的 PuzzleCore 解析结果。</returns>
        public static PuzzleInputResolveResult Resolve(PointerInputContext pointerContext)
        {
            PuzzleInputResolveResult result = default;
            InputHitResult[] hitResults = pointerContext.HitResults;
            if (hitResults == null || hitResults.Length == 0)
            {
                return result;
            }

            for (int i = 0; i < hitResults.Length; ++i)
            {
                Entity entity = hitResults[i].Entity;
                if (entity == null)
                {
                    continue;
                }

                if (result.PuzzleView == null && entity is PuzzleView puzzleView)
                {
                    result.PuzzleView = puzzleView;
                    result.Puzzle = puzzleView.GetParent<Puzzle>();
                }

                if (entity is SlotView slotView)
                {
                    Slot slot = slotView.GetParent<Slot>();
                    result.SlotView = result.SlotView ?? slotView;
                    result.Slot = result.Slot ?? slot;

                    if (slot != null && slot.IsPuzzleSlot())
                    {
                        result.PuzzleSlotView ??= slotView;
                        result.PuzzleSlot ??= slot;
                        if (result.Puzzle == null && slot.Parent is Puzzle puzzle)
                        {
                            result.Puzzle = puzzle;
                            result.PuzzleView = puzzle.GetComponent<PuzzleView>();
                        }
                    }

                    if (slot != null && slot.IsGridSlot())
                    {
                        result.GridSlotView ??= slotView;
                        result.GridSlot ??= slot;
                    }
                }

                if (result.PuzzleView != null && result.PuzzleSlotView != null && result.GridSlotView != null)
                {
                    return result;
                }
            }

            return result;
        }
    }
}
