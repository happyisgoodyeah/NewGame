using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// PuzzleDragComponent 的拖拽状态维护逻辑。
    /// </summary>
    [EntitySystemOf(typeof(PuzzleDragComponent))]
    public static partial class PuzzleDragComponentSystem
    {
        private const float FollowTweenDuration = 0.18f;
        private const float GridStepThreshold = 0.5f;

        /// <summary>
        /// 初始化拖拽状态。
        /// </summary>
        [EntitySystem]
        private static void Awake(this PuzzleDragComponent self)
        {
            self.IsDragging = false;
            self.DragStartWorldPosition = Vector3.zero;
            self.InitialWorldPosition = Vector3.zero;
            self.HasInitialWorldPosition = false;
            self.DragStartState = PuzzleState.Tray;
            self.DragStartAnchorX = 0;
            self.DragStartAnchorY = 0;
            self.LastPointerWorldPosition = Vector3.zero;
            self.MoveTween = null;
            self.SnapAnchorX = 0;
            self.SnapAnchorY = 0;
            self.IsGridSnapActive = false;
        }

        /// <summary>
        /// 清理拖拽状态。
        /// </summary>
        [EntitySystem]
        private static void Destroy(this PuzzleDragComponent self)
        {
            self.KillMoveTween();
            self.IsDragging = false;
            self.DragStartWorldPosition = Vector3.zero;
            self.InitialWorldPosition = Vector3.zero;
            self.HasInitialWorldPosition = false;
            self.DragStartState = PuzzleState.Tray;
            self.DragStartAnchorX = 0;
            self.DragStartAnchorY = 0;
            self.LastPointerWorldPosition = Vector3.zero;
            self.MoveTween = null;
            self.SnapAnchorX = 0;
            self.SnapAnchorY = 0;
            self.IsGridSnapActive = false;
        }

        /// <summary>
        /// 开始一次新的拼图拖拽。
        /// </summary>
        /// <param name="self">当前拼图的拖拽组件。</param>
        /// <param name="pointerContext">触发拖拽的输入上下文。</param>
        public static void BeginDrag(this PuzzleDragComponent self, PointerInputContext pointerContext)
        {
            Puzzle puzzle = self.GetParent<Puzzle>();
            PuzzleView puzzleView = puzzle.GetComponent<PuzzleView>();
            if (puzzleView == null || puzzleView.Transform == null)
            {
                return;
            }

            self.CacheInitialWorldPosition(puzzleView);
            self.IsDragging = true;
            self.IsGridSnapActive = false;
            self.DragStartWorldPosition = puzzleView.Transform.position;
            self.DragStartState = puzzle.State;
            self.DragStartAnchorX = puzzle.AnchorX;
            self.DragStartAnchorY = puzzle.AnchorY;
            self.LastPointerWorldPosition = pointerContext.WorldPosition;
            self.KillMoveTween();
            self.PlayMoveTween(puzzleView, pointerContext.WorldPosition);

            Grid grid = puzzle.Scene().GetGrid();
            if (grid != null)
            {
                puzzle.ClearPlacement(grid);
            }

            puzzle.State = PuzzleState.Dragging;
            puzzleView.MoveMode = PuzzleMoveMode.FreeFollow;
        }

        /// <summary>
        /// 根据当前指针位置推进拖拽中的拼图表现。
        /// </summary>
        /// <param name="self">当前拼图的拖拽组件。</param>
        /// <param name="pointerWorldPosition">当前指针的世界坐标。</param>
        public static void UpdateDrag(this PuzzleDragComponent self, Vector3 pointerWorldPosition)
        {
            if (!self.IsDragging)
            {
                return;
            }

            Puzzle puzzle = self.GetParent<Puzzle>();
            PuzzleView puzzleView = puzzle.GetComponent<PuzzleView>();
            Grid grid = puzzle.Scene().GetGrid();
            if (puzzleView == null || puzzleView.Transform == null || grid == null)
            {
                return;
            }

            self.LastPointerWorldPosition = pointerWorldPosition;
            if (self.IsGridSnapActive)
            {
                self.UpdateGridSnapDrag(grid, pointerWorldPosition);
                return;
            }

            self.UpdateFreeFollowDrag(grid, pointerWorldPosition);
        }

        /// <summary>
        /// 结束当前拼图拖拽，并将状态恢复到最小可用版本的默认值。
        /// </summary>
        /// <param name="self">当前拼图的拖拽组件。</param>
        /// <param name="pointerWorldPosition">结束拖拽时的指针世界坐标。</param>
        public static void EndDrag(this PuzzleDragComponent self, Vector3 pointerWorldPosition)
        {
            if (!self.IsDragging)
            {
                return;
            }

            self.LastPointerWorldPosition = pointerWorldPosition;
            self.ClearDragState();
        }

        /// <summary>
        /// 将当前拼图平滑吸附到其原点格对应的 Grid 中心点。
        /// </summary>
        /// <param name="self">当前拼图的拖拽组件。</param>
        /// <param name="grid">当前拼图所在的 Grid。</param>
        public static void SnapToAnchor(this PuzzleDragComponent self, Grid grid)
        {
            Puzzle puzzle = self.GetParent<Puzzle>();
            PuzzleView puzzleView = puzzle.GetComponent<PuzzleView>();
            GridView gridView = grid?.GetComponent<GridView>();
            Slot anchorGridSlot = grid?.GetSlot(puzzle.AnchorX, puzzle.AnchorY);
            if (puzzleView == null || puzzleView.Transform == null || gridView == null || anchorGridSlot == null)
            {
                self.ClearDragState();
                return;
            }

            self.PlayMoveTween(puzzleView, gridView.GetSlotWorldPosition(anchorGridSlot));
            self.ClearDragState();
        }

        /// <summary>
        /// 将当前拼图恢复到本次拖拽开始前的状态和位置。
        /// </summary>
        /// <param name="self">当前拼图的拖拽组件。</param>
        public static void RestoreDragStart(this PuzzleDragComponent self)
        {
            Puzzle puzzle = self.GetParent<Puzzle>();
            puzzle.State = self.DragStartState;
            puzzle.AnchorX = self.DragStartAnchorX;
            puzzle.AnchorY = self.DragStartAnchorY;

            PuzzleView puzzleView = puzzle.GetComponent<PuzzleView>();
            if (puzzleView != null && puzzleView.Transform != null)
            {
                self.PlayMoveTween(puzzleView, self.DragStartWorldPosition);
            }

            self.ClearDragState();
        }

        /// <summary>
        /// 将当前拼图恢复到初始生成时的位置，并回到 Tray 状态。
        /// </summary>
        /// <param name="self">当前拼图的拖拽组件。</param>
        public static void RestoreInitialPosition(this PuzzleDragComponent self)
        {
            Puzzle puzzle = self.GetParent<Puzzle>();
            puzzle.State = PuzzleState.Tray;
            puzzle.AnchorX = 0;
            puzzle.AnchorY = 0;

            PuzzleView puzzleView = puzzle.GetComponent<PuzzleView>();
            if (puzzleView != null && puzzleView.Transform != null && self.HasInitialWorldPosition)
            {
                self.PlayMoveTween(puzzleView, self.InitialWorldPosition);
            }

            self.ClearDragState();
        }

        /// <summary>
        /// 判断当前拖拽是否处于 GridSnap 模式。
        /// </summary>
        /// <param name="self">当前拼图的拖拽组件。</param>
        /// <returns>当前是否已进入 GridSnap 模式。</returns>
        public static bool IsInGridSnapMode(this PuzzleDragComponent self)
        {
            return self.IsDragging && self.IsGridSnapActive;
        }

        /// <summary>
        /// 在自由跟随模式下推进拖拽，并在触碰 Grid 后尝试切入 GridSnap。
        /// </summary>
        /// <param name="self">当前拼图的拖拽组件。</param>
        /// <param name="grid">关卡中的唯一 Grid。</param>
        /// <param name="pointerWorldPosition">当前指针世界坐标。</param>
        private static void UpdateFreeFollowDrag(this PuzzleDragComponent self, Grid grid, Vector3 pointerWorldPosition)
        {
            Puzzle puzzle = self.GetParent<Puzzle>();
            PuzzleView puzzleView = puzzle.GetComponent<PuzzleView>();
            if (puzzleView == null || puzzleView.Transform == null)
            {
                return;
            }

            self.PlayMoveTween(puzzleView, pointerWorldPosition);
            puzzle.State = PuzzleState.Dragging;
            puzzleView.MoveMode = PuzzleMoveMode.FreeFollow;

            if (!puzzle.TryResolveSnapTarget(grid, out PuzzleGridSnapTarget snapTarget))
            {
                return;
            }

            if (puzzle.CheckPlacement(grid, snapTarget.AnchorX, snapTarget.AnchorY, out List<Slot> matchedGridSlots) != PuzzlePlacementCheckResult.Success)
            {
                return;
            }

            puzzle.ApplyPlacement(grid, snapTarget.AnchorX, snapTarget.AnchorY, matchedGridSlots);
            self.SnapAnchorX = snapTarget.AnchorX;
            self.SnapAnchorY = snapTarget.AnchorY;
            self.IsGridSnapActive = true;
            puzzleView.MoveMode = PuzzleMoveMode.GridSnap;
            self.PlayMoveTween(puzzleView, grid.GetComponent<GridView>().GetSlotWorldPosition(snapTarget.GridSlot));
        }

        /// <summary>
        /// 在 GridSnap 模式下按格推进拖拽，并在向外越界时回退到自由跟随。
        /// </summary>
        /// <param name="self">当前拼图的拖拽组件。</param>
        /// <param name="grid">关卡中的唯一 Grid。</param>
        /// <param name="pointerWorldPosition">当前指针世界坐标。</param>
        private static void UpdateGridSnapDrag(this PuzzleDragComponent self, Grid grid, Vector3 pointerWorldPosition)
        {
            Puzzle puzzle = self.GetParent<Puzzle>();
            PuzzleView puzzleView = puzzle.GetComponent<PuzzleView>();
            GridView gridView = grid.GetComponent<GridView>();
            Slot currentAnchorSlot = grid.GetSlot(self.SnapAnchorX, self.SnapAnchorY);
            if (puzzleView == null || gridView == null || currentAnchorSlot == null)
            {
                return;
            }

            Vector3 anchorWorldPosition = gridView.GetSlotWorldPosition(currentAnchorSlot);
            Vector3 deltaWorldPosition = pointerWorldPosition - anchorWorldPosition;
            int deltaX = ResolveStepDelta(deltaWorldPosition.x, gridView.CellSize);
            int deltaY = ResolveStepDelta(-deltaWorldPosition.y, gridView.CellSize);
            if (deltaX == 0 && deltaY == 0)
            {
                return;
            }

            int targetAnchorX = self.SnapAnchorX + deltaX;
            int targetAnchorY = self.SnapAnchorY + deltaY;
            puzzle.ClearPlacement(grid);
            PuzzlePlacementCheckResult checkResult = puzzle.CheckPlacement(grid, targetAnchorX, targetAnchorY, out List<Slot> matchedGridSlots);
            if (checkResult == PuzzlePlacementCheckResult.Success)
            {
                puzzle.ApplyPlacement(grid, targetAnchorX, targetAnchorY, matchedGridSlots);
                self.SnapAnchorX = targetAnchorX;
                self.SnapAnchorY = targetAnchorY;
                puzzleView.MoveMode = PuzzleMoveMode.GridSnap;
                self.PlayMoveTween(puzzleView, gridView.GetSlotWorldPosition(grid.GetSlot(targetAnchorX, targetAnchorY)));
                return;
            }

            if (checkResult == PuzzlePlacementCheckResult.OutOfGrid)
            {
                puzzle.State = PuzzleState.Dragging;
                puzzleView.MoveMode = PuzzleMoveMode.FreeFollow;
                self.IsGridSnapActive = false;
                self.PlayMoveTween(puzzleView, pointerWorldPosition);
                return;
            }

            if (puzzle.CheckPlacement(grid, self.SnapAnchorX, self.SnapAnchorY, out List<Slot> restoreSlots) == PuzzlePlacementCheckResult.Success)
            {
                puzzle.ApplyPlacement(grid, self.SnapAnchorX, self.SnapAnchorY, restoreSlots);
            }
        }

        /// <summary>
        /// 将当前拖拽中的 GridSnap 占用固化为最终放置状态。
        /// </summary>
        /// <param name="self">当前拼图的拖拽组件。</param>
        public static void FinalizeGridSnap(this PuzzleDragComponent self)
        {
            Puzzle puzzle = self.GetParent<Puzzle>();
            if (self.IsGridSnapActive)
            {
                puzzle.State = PuzzleState.Placed;
                puzzle.AnchorX = self.SnapAnchorX;
                puzzle.AnchorY = self.SnapAnchorY;
            }

            self.ClearDragState();
        }

        /// <summary>
        /// 将目标世界坐标转换为 PuzzleView 父节点局部坐标后应用到表现层。
        /// </summary>
        /// <param name="self">当前拼图的拖拽组件。</param>
        /// <param name="puzzleView">要被移动的拼图表现层。</param>
        /// <param name="targetWorldPosition">目标世界坐标。</param>
        private static void PlayMoveTween(this PuzzleDragComponent self, PuzzleView puzzleView, Vector3 targetWorldPosition)
        {
            Transform parentTransform = puzzleView.Transform.parent;
            Vector3 targetLocalPosition = parentTransform != null ? parentTransform.InverseTransformPoint(targetWorldPosition) : targetWorldPosition;
            self.KillMoveTween();
            self.MoveTween = puzzleView.Transform
                    .DOLocalMove(targetLocalPosition, FollowTweenDuration)
                    .SetEase(Ease.OutQuad)
                    .SetLink(puzzleView.GameObject);
        }

        /// <summary>
        /// 在第一次参与拖拽时缓存 Puzzle 的初始生成位置。
        /// </summary>
        /// <param name="self">当前拼图的拖拽组件。</param>
        /// <param name="puzzleView">当前拼图的表现层。</param>
        private static void CacheInitialWorldPosition(this PuzzleDragComponent self, PuzzleView puzzleView)
        {
            if (self.HasInitialWorldPosition || puzzleView == null || puzzleView.Transform == null)
            {
                return;
            }

            self.InitialWorldPosition = puzzleView.Transform.position;
            self.HasInitialWorldPosition = true;
        }

        /// <summary>
        /// 解析某个轴在 GridSnap 模式下是否应走出一步。
        /// </summary>
        /// <param name="axisDelta">当前鼠标相对原点格中心在该轴上的位移。</param>
        /// <param name="cellSize">当前格子的边长。</param>
        /// <returns>本帧该轴的离散步进值。</returns>
        private static int ResolveStepDelta(float axisDelta, float cellSize)
        {
            float threshold = cellSize * GridStepThreshold;
            if (axisDelta > threshold)
            {
                return 1;
            }

            if (axisDelta < -threshold)
            {
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// 停止当前仍在运行的移动 Tween。
        /// </summary>
        /// <param name="self">当前拼图的拖拽组件。</param>
        private static void KillMoveTween(this PuzzleDragComponent self)
        {
            if (self.MoveTween == null)
            {
                return;
            }

            if (self.MoveTween.IsActive())
            {
                self.MoveTween.Kill(false);
            }

            self.MoveTween = null;
        }

        /// <summary>
        /// 清理一次拖拽结束后的组件状态。
        /// </summary>
        /// <param name="self">当前拼图的拖拽组件。</param>
        private static void ClearDragState(this PuzzleDragComponent self)
        {
            self.IsDragging = false;
            self.DragStartWorldPosition = Vector3.zero;
            self.DragStartState = PuzzleState.Tray;
            self.DragStartAnchorX = 0;
            self.DragStartAnchorY = 0;
            self.LastPointerWorldPosition = Vector3.zero;
            self.SnapAnchorX = 0;
            self.SnapAnchorY = 0;
            self.IsGridSnapActive = false;
        }
    }
}
