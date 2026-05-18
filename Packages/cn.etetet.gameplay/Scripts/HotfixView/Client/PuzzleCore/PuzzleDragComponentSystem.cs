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
        /// <summary>
        /// 自由跟随指针时的移动缓动时长
        /// </summary>
        private const float FreeFollowTweenDuration = 0.08f;

        /// <summary>
        /// 吸附预览移动到候选格的缓动时长
        /// </summary>
        private const float SnapTweenDuration = 0.145f;

        /// <summary>
        /// 吸附预览使用 OutBack 曲线时的回弹强度
        /// </summary>
        private const float SnapEaseOvershoot = 0.72f;

        /// <summary>
        /// 拖拽结束后落位或回退的缓动时长
        /// </summary>
        private const float SettleTweenDuration = 0.12f;

        /// <summary>
        /// 吸附模式下指针触发格子步进的半格阈值
        /// </summary>
        private const float SnapStepThreshold = 0.5f;

        /// <summary>
        /// 判断移动目标是否重复时使用的平方距离容差
        /// </summary>
        private const float MoveTargetEpsilonSqr = 0.000001f;

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
            self.MoveTargetLocalPosition = Vector3.zero;
            self.HasMoveTargetLocalPosition = false;
            self.SnapAnchorX = 0;
            self.SnapAnchorY = 0;
            self.SnapRegion = (byte)GridContactRegion.Center;
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
            self.MoveTargetLocalPosition = Vector3.zero;
            self.HasMoveTargetLocalPosition = false;
            self.SnapAnchorX = 0;
            self.SnapAnchorY = 0;
            self.SnapRegion = (byte)GridContactRegion.Center;
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

            puzzleView.BringToFront();

            Grid grid = puzzle.Scene().GetGrid();
            if (grid != null && puzzle.State == PuzzleState.Placed)
            {
                puzzle.ClearPlacement(grid);
                self.SnapAnchorX = puzzle.AnchorX;
                self.SnapAnchorY = puzzle.AnchorY;
                self.SnapRegion = (byte)GridContactRegion.Center;
                self.IsGridSnapActive = true;
                puzzle.State = PuzzleState.Dragging;
                puzzleView.MoveMode = PuzzleMoveMode.GridSnap;
                return;
            }

            self.PlayMoveTween(puzzleView, pointerContext.WorldPosition, FreeFollowTweenDuration, Ease.OutQuad);
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
        /// 判断当前 GridSnap 预览是否已经进入真正覆盖 Slot 的可提交状态。
        /// </summary>
        /// <param name="self">当前拼图的拖拽组件。</param>
        /// <returns>当前是否允许在松手时提交放置。</returns>
        public static bool CanFinalizeGridSnap(this PuzzleDragComponent self)
        {
            if (!self.IsGridSnapActive)
            {
                return false;
            }

            Puzzle puzzle = self.GetParent<Puzzle>();
            Grid grid = puzzle?.Scene().GetGrid();
            return puzzle != null
                    && grid != null
                    && puzzle.CheckPlacement(grid, self.SnapAnchorX, self.SnapAnchorY, out List<Slot> _) == PuzzlePlacementCheckResult.Success;
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
            int anchorX = self.IsGridSnapActive ? self.SnapAnchorX : puzzle.AnchorX;
            int anchorY = self.IsGridSnapActive ? self.SnapAnchorY : puzzle.AnchorY;
            if (puzzleView == null || puzzleView.Transform == null || gridView == null)
            {
                self.ClearDragState();
                return;
            }

            if (puzzle.CheckPlacement(grid, anchorX, anchorY, out List<Slot> matchedGridSlots) != PuzzlePlacementCheckResult.Success)
            {
                self.ClearDragState();
                return;
            }

            if (self.IsGridSnapActive || puzzle.State != PuzzleState.Placed)
            {
                puzzle.ApplyPlacement(grid, anchorX, anchorY, matchedGridSlots);
            }

            puzzle.State = PuzzleState.Placed;
            puzzleView.RestoreVisualPriority();
            self.PlayMoveTweenToOrigin(puzzleView, gridView.GetGridCoordinateWorldPosition(anchorX, anchorY), SettleTweenDuration, Ease.OutCubic);
            self.ClearDragState();
        }

        /// <summary>
        /// 将当前拼图恢复到本次拖拽开始前的状态和位置。
        /// </summary>
        /// <param name="self">当前拼图的拖拽组件。</param>
        public static void RestoreDragStart(this PuzzleDragComponent self)
        {
            Puzzle puzzle = self.GetParent<Puzzle>();
            self.ClearPreviewPlacementReservation();
            puzzle.State = self.DragStartState;
            puzzle.AnchorX = self.DragStartAnchorX;
            puzzle.AnchorY = self.DragStartAnchorY;

            PuzzleView puzzleView = puzzle.GetComponent<PuzzleView>();
            if (puzzleView != null && puzzleView.Transform != null)
            {
                puzzleView.RestoreVisualPriority();
                self.PlayMoveTween(puzzleView, self.DragStartWorldPosition, SettleTweenDuration, Ease.OutCubic);
            }

            self.ClearDragState();
        }

        /// <summary>
        /// 将当前拼图恢复到初始生成时的位置，并重置为默认朝向。
        /// </summary>
        /// <param name="self">当前拼图的拖拽组件。</param>
        public static void RestoreInitialPosition(this PuzzleDragComponent self)
        {
            Puzzle puzzle = self.GetParent<Puzzle>();
            self.ClearPreviewPlacementReservation();
            puzzle.State = PuzzleState.Tray;
            puzzle.AnchorX = 0;
            puzzle.AnchorY = 0;
            puzzle.SetRotation(PuzzleRotation.Rotate0);

            PuzzleView puzzleView = puzzle.GetComponent<PuzzleView>();
            if (puzzleView != null && puzzleView.Transform != null)
            {
                puzzleView.RestoreVisualPriority();
                puzzleView.RefreshRotation();
                if (self.HasInitialWorldPosition)
                {
                    self.PlayMoveTween(puzzleView, self.InitialWorldPosition, SettleTweenDuration, Ease.OutCubic);
                }
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
            GridView gridView = grid.GetComponent<GridView>();
            if (puzzleView == null || puzzleView.Transform == null || gridView == null)
            {
                return;
            }

            // 未接触 Grid 前只让 Puzzle 平滑追随指针
            self.PlayMoveTween(puzzleView, pointerWorldPosition, FreeFollowTweenDuration, Ease.OutQuad);
            puzzle.State = PuzzleState.Dragging;
            puzzleView.MoveMode = PuzzleMoveMode.FreeFollow;

            // 首次接触 Grid 时解析一个候选吸附锚点，解析失败则继续自由跟随
            if (!puzzle.TryResolveEntrySnapTarget(grid, out PuzzleGridSnapTarget snapTarget))
            {
                return;
            }

            // 切入 GridSnap 后由离散锚点驱动后续拖拽移动
            puzzle.ClearPlacement(grid);
            self.SnapAnchorX = snapTarget.AnchorX;
            self.SnapAnchorY = snapTarget.AnchorY;
            self.SnapRegion = (byte)snapTarget.Region;
            self.IsGridSnapActive = true;
            puzzleView.MoveMode = PuzzleMoveMode.GridSnap;
            self.ApplyPreviewAnchorPosition(puzzleView, gridView, snapTarget.AnchorX, snapTarget.AnchorY);
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
            if (puzzleView == null || gridView == null)
            {
                return;
            }

            Vector3 snapReferenceWorldPosition = gridView.GetGridCoordinateWorldPosition(self.SnapAnchorX, self.SnapAnchorY);
            if (!puzzle.IsFullyInsideGrid(grid) && !puzzle.IsPointerInsideAdsorptionRange(grid, gridView, pointerWorldPosition))
            {
                puzzle.ClearPlacement(grid);
                puzzle.State = PuzzleState.Dragging;
                puzzleView.MoveMode = PuzzleMoveMode.FreeFollow;
                self.IsGridSnapActive = false;
                self.PlayMoveTween(puzzleView, pointerWorldPosition, FreeFollowTweenDuration, Ease.OutQuad);
                return;
            }

            if (IsPointInsideSnapRange(snapReferenceWorldPosition, pointerWorldPosition, gridView.CellSize))
            {
                self.ApplyPreviewAnchorPosition(puzzleView, gridView, self.SnapAnchorX, self.SnapAnchorY);
                return;
            }

            PuzzleSnapDirection direction = PuzzleGridSnapResolver.ResolveSnapDirection(snapReferenceWorldPosition, pointerWorldPosition, true);
            PuzzleGridSnapResolver.GetDirectionDelta(direction, out int deltaX, out int deltaY);
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
                self.ApplyPreviewPlacement(puzzle, grid, targetAnchorX, targetAnchorY, matchedGridSlots);
                self.SnapAnchorX = targetAnchorX;
                self.SnapAnchorY = targetAnchorY;
                self.SnapRegion = (byte)GridContactRegion.Center;
                puzzleView.MoveMode = PuzzleMoveMode.GridSnap;
                self.ApplyPreviewAnchorPosition(puzzleView, gridView, targetAnchorX, targetAnchorY);
                return;
            }

            if (puzzle.IsPointerInsideAdsorptionRange(grid, gridView, pointerWorldPosition))
            {
                self.SnapAnchorX = targetAnchorX;
                self.SnapAnchorY = targetAnchorY;
                self.SnapRegion = (byte)ResolveAnchorRegion(grid, targetAnchorX, targetAnchorY);
                puzzle.State = PuzzleState.Dragging;
                puzzleView.MoveMode = PuzzleMoveMode.GridSnap;
                self.ApplyPreviewAnchorPosition(puzzleView, gridView, targetAnchorX, targetAnchorY);
                return;
            }

            puzzle.State = PuzzleState.Dragging;
            puzzleView.MoveMode = PuzzleMoveMode.FreeFollow;
            self.IsGridSnapActive = false;
            self.PlayMoveTween(puzzleView, pointerWorldPosition, FreeFollowTweenDuration, Ease.OutQuad);
        }

        /// <summary>
        /// 将当前拖拽中的 GridSnap 占用固化为最终放置状态。
        /// </summary>
        /// <param name="self">当前拼图的拖拽组件。</param>
        public static void FinalizeGridSnap(this PuzzleDragComponent self)
        {
            Puzzle puzzle = self.GetParent<Puzzle>();
            Grid grid = puzzle?.Scene().GetGrid();
            if (self.IsGridSnapActive
                    && grid != null
                    && puzzle.CheckPlacement(grid, self.SnapAnchorX, self.SnapAnchorY, out List<Slot> matchedGridSlots) == PuzzlePlacementCheckResult.Success)
            {
                puzzle.ApplyPlacement(grid, self.SnapAnchorX, self.SnapAnchorY, matchedGridSlots);
            }

            self.ClearDragState();
        }

        /// <summary>
        /// 将目标世界坐标转换为 PuzzleView 父节点局部坐标后用 DOTween 缓动到目标。
        /// </summary>
        /// <param name="self">当前拼图的拖拽组件。</param>
        /// <param name="puzzleView">要被移动的拼图表现层。</param>
        /// <param name="targetWorldPosition">目标世界坐标。</param>
        /// <param name="duration">缓动持续时间。</param>
        /// <param name="ease">缓动曲线。</param>
        /// <param name="overshoot">Back/Elastic 类曲线使用的回弹强度，0 表示使用 DOTween 默认曲线参数。</param>
        private static void PlayMoveTween(this PuzzleDragComponent self, PuzzleView puzzleView, Vector3 targetWorldPosition, float duration, Ease ease, float overshoot = 0f)
        {
            Transform parentTransform = puzzleView.Transform.parent;
            // Tween 走 localPosition，先把世界目标折算到 PuzzleView 父节点空间
            Vector3 targetLocalPosition = parentTransform != null ? parentTransform.InverseTransformPoint(targetWorldPosition) : targetWorldPosition;
            //判断目标是否变化
            bool isSameTarget = self.HasMoveTargetLocalPosition && (self.MoveTargetLocalPosition - targetLocalPosition).sqrMagnitude <= MoveTargetEpsilonSqr;
            if (self.MoveTween != null && self.MoveTween.IsActive())
            {
                // 高频拖拽更新中目标未变化时不重启 Tween
                if (isSameTarget)
                {
                    return;
                }

                // 复用当前 Tween 改终点，避免每帧创建和销毁 Tween
                self.MoveTargetLocalPosition = targetLocalPosition;
                self.HasMoveTargetLocalPosition = true;
                self.MoveTween.ChangeEndValue(targetLocalPosition, duration, true);
                self.ApplyMoveEase(ease, overshoot);
                self.MoveTween.Restart();
                return;
            }

            self.KillMoveTween();
            self.MoveTargetLocalPosition = targetLocalPosition;
            self.HasMoveTargetLocalPosition = true;
            self.MoveTween = puzzleView.Transform
                    .DOLocalMove(targetLocalPosition, duration)
                    .SetLink(puzzleView.GameObject);
            self.ApplyMoveEase(ease, overshoot);
        }

        /// <summary>
        /// 将目标原点格世界坐标转换为 PuzzleView 根节点轨迹后应用 Tween。
        /// </summary>
        /// <param name="self">当前拼图的拖拽组件。</param>
        /// <param name="puzzleView">要被移动的拼图表现层。</param>
        /// <param name="targetOriginWorldPosition">目标原点格世界坐标。</param>
        /// <param name="duration">缓动持续时间。</param>
        /// <param name="ease">缓动曲线。</param>
        /// <param name="overshoot">Back/Elastic 类曲线使用的回弹强度，0 表示使用 DOTween 默认曲线参数。</param>
        private static void PlayMoveTweenToOrigin(this PuzzleDragComponent self, PuzzleView puzzleView, Vector3 targetOriginWorldPosition, float duration, Ease ease, float overshoot = 0f)
        {
            Vector3 currentOriginWorldPosition = puzzleView.GetOriginWorldPosition();
            Vector3 rootTargetWorldPosition = puzzleView.Transform.position + (targetOriginWorldPosition - currentOriginWorldPosition);
            self.PlayMoveTween(puzzleView, rootTargetWorldPosition, duration, ease, overshoot);
        }

        /// <summary>
        /// 为当前移动 Tween 应用指定缓动曲线，并在需要时设置回弹强度。
        /// </summary>
        /// <param name="self">当前拼图的拖拽组件。</param>
        /// <param name="ease">缓动曲线。</param>
        /// <param name="overshoot">Back/Elastic 类曲线使用的回弹强度，0 表示使用 DOTween 默认曲线参数。</param>
        private static void ApplyMoveEase(this PuzzleDragComponent self, Ease ease, float overshoot)
        {
            if (self.MoveTween == null)
            {
                return;
            }

            if (overshoot > 0f)
            {
                self.MoveTween.SetEase(ease, overshoot);
                return;
            }

            self.MoveTween.SetEase(ease);
        }

        /// <summary>
        /// 将当前拼图原点吸附到指定 Grid 离散坐标，坐标允许落在 Grid 外用于外圈预览。
        /// </summary>
        /// <param name="self">当前拼图的拖拽组件。</param>
        /// <param name="puzzleView">要被移动的拼图表现层。</param>
        /// <param name="gridView">目标 Grid 的表现层。</param>
        /// <param name="anchorX">候选原点在 Grid 坐标系中的 X 坐标。</param>
        /// <param name="anchorY">候选原点在 Grid 坐标系中的 Y 坐标。</param>
        private static void ApplyPreviewAnchorPosition(this PuzzleDragComponent self, PuzzleView puzzleView, GridView gridView, int anchorX, int anchorY)
        {
            Vector3 previewWorldPosition = gridView.GetGridCoordinateWorldPosition(anchorX, anchorY);
            self.PlayMoveTweenToOrigin(puzzleView, previewWorldPosition, SnapTweenDuration, Ease.OutBack, SnapEaseOvershoot);
        }

        /// <summary>
        /// 根据当前原点坐标相对 Grid 边界的位置，折算为外圈吸附区域。
        /// </summary>
        /// <param name="grid">目标 Grid。</param>
        /// <param name="anchorX">当前原点 X 坐标。</param>
        /// <param name="anchorY">当前原点 Y 坐标。</param>
        /// <returns>当前原点所处的边界区域。</returns>
        private static GridContactRegion ResolveAnchorRegion(Grid grid, int anchorX, int anchorY)
        {
            if (grid == null || grid.Contains(anchorX, anchorY))
            {
                return GridContactRegion.Center;
            }

            bool isLeft = anchorX < 0;
            bool isRight = anchorX >= grid.Width;
            bool isTop = anchorY < 0;
            bool isBottom = anchorY >= grid.Height;
            if (isLeft && isTop)
            {
                return GridContactRegion.LeftTop;
            }

            if (isRight && isTop)
            {
                return GridContactRegion.RightTop;
            }

            if (isLeft && isBottom)
            {
                return GridContactRegion.LeftBottom;
            }

            if (isRight && isBottom)
            {
                return GridContactRegion.RightBottom;
            }

            if (isLeft)
            {
                return GridContactRegion.Left;
            }

            if (isRight)
            {
                return GridContactRegion.Right;
            }

            if (isTop)
            {
                return GridContactRegion.Top;
            }

            return GridContactRegion.Bottom;
        }

        /// <summary>
        /// 将一组合法格位作为拖拽中的预览占用应用到 Grid，并保持 Puzzle 仍处于 Dragging 状态。
        /// </summary>
        /// <param name="self">当前拼图的拖拽组件。</param>
        /// <param name="puzzle">当前正在拖拽的 Puzzle。</param>
        /// <param name="grid">目标 Grid。</param>
        /// <param name="anchorX">预览原点格的 X 坐标。</param>
        /// <param name="anchorY">预览原点格的 Y 坐标。</param>
        /// <param name="matchedGridSlots">本次预览占用涉及的 Grid Slot 集合。</param>
        private static void ApplyPreviewPlacement(this PuzzleDragComponent self, Puzzle puzzle, Grid grid, int anchorX, int anchorY, List<Slot> matchedGridSlots)
        {
            puzzle.ApplyPlacement(grid, anchorX, anchorY, matchedGridSlots);
            puzzle.State = PuzzleState.Dragging;
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
        /// 清理当前拖拽过程在 Grid 上保留的预览占用。
        /// </summary>
        /// <param name="self">当前拼图的拖拽组件。</param>
        private static void ClearPreviewPlacementReservation(this PuzzleDragComponent self)
        {
            Puzzle puzzle = self.GetParent<Puzzle>();
            Grid grid = puzzle?.Scene().GetGrid();
            if (puzzle == null || grid == null)
            {
                return;
            }

            puzzle.ClearPlacement(grid);
        }

        /// <summary>
        /// 判断指针是否仍处于当前吸附参考点的半格范围内。
        /// </summary>
        /// <param name="originWorldPosition">吸附参考世界坐标。</param>
        /// <param name="pointerWorldPosition">当前指针世界坐标。</param>
        /// <param name="cellSize">当前格子边长。</param>
        /// <returns>指针是否还没有越过步进阈值。</returns>
        private static bool IsPointInsideSnapRange(Vector3 originWorldPosition, Vector3 pointerWorldPosition, float cellSize)
        {
            float threshold = cellSize * SnapStepThreshold;
            return pointerWorldPosition.x >= originWorldPosition.x - threshold
                    && pointerWorldPosition.x <= originWorldPosition.x + threshold
                    && pointerWorldPosition.y >= originWorldPosition.y - threshold
                    && pointerWorldPosition.y <= originWorldPosition.y + threshold;
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
            self.MoveTargetLocalPosition = Vector3.zero;
            self.HasMoveTargetLocalPosition = false;
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
            self.SnapRegion = (byte)GridContactRegion.Center;
            self.IsGridSnapActive = false;
        }
    }
}
