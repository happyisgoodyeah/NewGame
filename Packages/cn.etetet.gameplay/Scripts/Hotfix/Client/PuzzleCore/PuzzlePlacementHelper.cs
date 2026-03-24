using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Puzzle 基于离散格坐标的放置与回退辅助方法。
    /// </summary>
    public static class PuzzlePlacementHelper
    {
        /// <summary>
        /// 尝试让当前 Puzzle 以指定 Grid Slot 作为原点格进行放置。
        /// </summary>
        /// <param name="self">要放置的 Puzzle。</param>
        /// <param name="grid">目标 Grid。</param>
        /// <param name="originGridSlot">鼠标松开时命中的原点目标 Grid Slot。</param>
        /// <returns>当前拼图是否成功放置。</returns>
        public static bool TryPlaceOnGrid(this Puzzle self, Grid grid, Slot originGridSlot)
        {
            if (self.CheckPlacement(grid, originGridSlot, out List<Slot> matchedGridSlots) != PuzzlePlacementCheckResult.Success)
            {
                return false;
            }

            self.ApplyPlacement(grid, originGridSlot.X, originGridSlot.Y, matchedGridSlots);
            return true;
        }

        /// <summary>
        /// 按指定原点格坐标尝试恢复或放置当前 Puzzle。
        /// </summary>
        /// <param name="self">要放置的 Puzzle。</param>
        /// <param name="grid">目标 Grid。</param>
        /// <param name="anchorX">原点格的 X 坐标。</param>
        /// <param name="anchorY">原点格的 Y 坐标。</param>
        /// <returns>当前拼图是否成功放置。</returns>
        public static bool TryPlaceOnGrid(this Puzzle self, Grid grid, int anchorX, int anchorY)
        {
            if (self.CheckPlacement(grid, anchorX, anchorY, out List<Slot> matchedGridSlots) != PuzzlePlacementCheckResult.Success)
            {
                return false;
            }

            self.ApplyPlacement(grid, anchorX, anchorY, matchedGridSlots);
            return true;
        }

        /// <summary>
        /// 检查当前 Puzzle 以指定 Grid Slot 作为原点格时的放置结果。
        /// </summary>
        /// <param name="self">要检查的 Puzzle。</param>
        /// <param name="grid">目标 Grid。</param>
        /// <param name="originGridSlot">原点目标格。</param>
        /// <param name="matchedGridSlots">返回全部匹配到的目标 Grid Slot。</param>
        /// <returns>本次放置判定结果。</returns>
        public static PuzzlePlacementCheckResult CheckPlacement(this Puzzle self, Grid grid, Slot originGridSlot, out List<Slot> matchedGridSlots)
        {
            matchedGridSlots = new List<Slot>();
            if (grid == null || originGridSlot == null || !originGridSlot.IsGridSlot())
            {
                return PuzzlePlacementCheckResult.Invalid;
            }

            return self.CheckPlacement(grid, originGridSlot.X, originGridSlot.Y, out matchedGridSlots);
        }

        /// <summary>
        /// 检查当前 Puzzle 以指定原点格坐标放置时的判定结果。
        /// </summary>
        /// <param name="self">要检查的 Puzzle。</param>
        /// <param name="grid">目标 Grid。</param>
        /// <param name="anchorX">原点格的 X 坐标。</param>
        /// <param name="anchorY">原点格的 Y 坐标。</param>
        /// <param name="matchedGridSlots">返回全部匹配到的目标 Grid Slot。</param>
        /// <returns>本次放置判定结果。</returns>
        public static PuzzlePlacementCheckResult CheckPlacement(this Puzzle self, Grid grid, int anchorX, int anchorY, out List<Slot> matchedGridSlots)
        {
            matchedGridSlots = new List<Slot>();
            Slot originSlot = self.GetOriginSlot();
            if (grid == null || originSlot == null || self.ChildrenCount() <= 0)
            {
                return PuzzlePlacementCheckResult.Invalid;
            }

            foreach (Entity child in self.Children.Values)
            {
                Slot puzzleSlot = child as Slot;
                if (puzzleSlot == null)
                {
                    continue;
                }

                self.GetRotatedOffset(puzzleSlot, out int offsetX, out int offsetY);
                int gridX = anchorX + offsetX;
                int gridY = anchorY + offsetY;
                if (!grid.Contains(gridX, gridY))
                {
                    matchedGridSlots.Clear();
                    return PuzzlePlacementCheckResult.OutOfGrid;
                }

                Slot gridSlot = grid.GetSlot(gridX, gridY);
                if (gridSlot == null)
                {
                    matchedGridSlots.Clear();
                    return PuzzlePlacementCheckResult.Invalid;
                }

                GridSlotStateComponent stateComponent = gridSlot.GetComponent<GridSlotStateComponent>();
                if (stateComponent == null)
                {
                    matchedGridSlots.Clear();
                    return PuzzlePlacementCheckResult.Invalid;
                }

                if (gridSlot.Kind != SlotType.GridPlaceable)
                {
                    matchedGridSlots.Clear();
                    return PuzzlePlacementCheckResult.Blocked;
                }

                Puzzle bindingPuzzle = gridSlot.GetBindingPuzzle();
                if (bindingPuzzle != null && bindingPuzzle != self)
                {
                    matchedGridSlots.Clear();
                    return PuzzlePlacementCheckResult.Occupied;
                }

                if (bindingPuzzle == null && !stateComponent.CanPlacePuzzle())
                {
                    matchedGridSlots.Clear();
                    return PuzzlePlacementCheckResult.Blocked;
                }

                matchedGridSlots.Add(gridSlot);
            }

            return matchedGridSlots.Count > 0 ? PuzzlePlacementCheckResult.Success : PuzzlePlacementCheckResult.Invalid;
        }

        /// <summary>
        /// 清理当前 Puzzle 在 Grid 上已记录的占用信息。
        /// </summary>
        /// <param name="self">要清理占用的 Puzzle。</param>
        /// <param name="grid">当前 Puzzle 所在的 Grid。</param>
        public static void ClearPlacement(this Puzzle self, Grid grid)
        {
            if (grid == null || self.ChildrenCount() <= 0)
            {
                return;
            }

            foreach (Entity child in self.Children.Values)
            {
                Slot puzzleSlot = child as Slot;
                if (puzzleSlot == null)
                {
                    continue;
                }

                self.GetRotatedOffset(puzzleSlot, out int offsetX, out int offsetY);
                Slot gridSlot = grid.GetSlot(self.AnchorX + offsetX, self.AnchorY + offsetY);
                if (gridSlot != null && gridSlot.IsBoundToPuzzle(self))
                {
                    gridSlot.ClearBindingPuzzle();
                }
            }

            grid.RefreshStatistics();
        }

        /// <summary>
        /// 将一组已通过合法性校验的目标 Grid Slot 应用为当前 Puzzle 的放置结果。
        /// </summary>
        /// <param name="self">要执行放置的 Puzzle。</param>
        /// <param name="grid">目标 Grid。</param>
        /// <param name="anchorX">原点格的 X 坐标。</param>
        /// <param name="anchorY">原点格的 Y 坐标。</param>
        /// <param name="matchedGridSlots">本次放置涉及的全部目标 Grid Slot。</param>
        public static void ApplyPlacement(this Puzzle self, Grid grid, int anchorX, int anchorY, List<Slot> matchedGridSlots)
        {
            if (grid == null || matchedGridSlots == null || matchedGridSlots.Count == 0)
            {
                return;
            }

            foreach (Slot gridSlot in matchedGridSlots)
            {
                gridSlot.BindPuzzle(self);
            }

            self.SetAnchor(anchorX, anchorY);
            self.State = PuzzleState.Placed;
            grid.RefreshStatistics();
        }
    }
}
