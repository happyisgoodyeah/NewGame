using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// GridSnap 模式下的接触区域类型。
    /// </summary>
    public enum GridContactRegion : byte
    {
        /// <summary>
        /// 非边界或无需特殊吸附的区域。
        /// </summary>
        Center = 0,

        /// <summary>
        /// 左边界区域。
        /// </summary>
        Left = 1,

        /// <summary>
        /// 右边界区域。
        /// </summary>
        Right = 2,

        /// <summary>
        /// 上边界区域。
        /// </summary>
        Top = 3,

        /// <summary>
        /// 下边界区域。
        /// </summary>
        Bottom = 4,

        /// <summary>
        /// 左上角区域。
        /// </summary>
        LeftTop = 5,

        /// <summary>
        /// 右上角区域。
        /// </summary>
        RightTop = 6,

        /// <summary>
        /// 左下角区域。
        /// </summary>
        LeftBottom = 7,

        /// <summary>
        /// 右下角区域。
        /// </summary>
        RightBottom = 8,
    }

    /// <summary>
    /// 吸附移动使用的离散方向。
    /// </summary>
    public enum PuzzleSnapDirection : byte
    {
        /// <summary>
        /// 未解析到有效方向。
        /// </summary>
        None = 0,

        /// <summary>
        /// 向上。
        /// </summary>
        Up = 1,

        /// <summary>
        /// 向下。
        /// </summary>
        Down = 2,

        /// <summary>
        /// 向左。
        /// </summary>
        Left = 3,

        /// <summary>
        /// 向右。
        /// </summary>
        Right = 4,

        /// <summary>
        /// 向左上。
        /// </summary>
        LeftTop = 5,

        /// <summary>
        /// 向右上。
        /// </summary>
        RightTop = 6,

        /// <summary>
        /// 向左下。
        /// </summary>
        LeftBottom = 7,

        /// <summary>
        /// 向右下。
        /// </summary>
        RightBottom = 8,
    }

    /// <summary>
    /// 一次 Grid 接触解析得到的候选吸附目标。
    /// </summary>
    public struct PuzzleGridSnapTarget
    {
        /// <summary>
        /// 候选原点格。
        /// </summary>
        public Slot GridSlot;

        /// <summary>
        /// 候选原点在 Grid 坐标系中的 X 坐标，初次外圈吸附时允许落在 Grid 外。
        /// </summary>
        public int AnchorX;

        /// <summary>
        /// 候选原点在 Grid 坐标系中的 Y 坐标，初次外圈吸附时允许落在 Grid 外。
        /// </summary>
        public int AnchorY;

        /// <summary>
        /// 当前接触区域类型。
        /// </summary>
        public GridContactRegion Region;
    }

    /// <summary>
    /// Puzzle 与 Grid 接触后的候选吸附解析辅助方法。
    /// </summary>
    public static class PuzzleGridSnapResolver
    {
        /// <summary>
        /// 角落格内区分角吸附和边吸附的三分区阈值
        /// </summary>
        private const float CornerThirdThreshold = 1f / 3f;

        /// <summary>
        /// 八方向判定时每个方向边界使用的半扇区角度
        /// </summary>
        private const float DirectionSectorAngle = 22.5f;

        /// <summary>
        /// 尝试根据当前 Puzzle 主体与 Grid 的接触情况解析候选原点格。
        /// </summary>
        /// <param name="puzzle">当前拖拽中的 Puzzle。</param>
        /// <param name="grid">关卡中的唯一 Grid。</param>
        /// <param name="snapTarget">返回候选吸附目标。</param>
        /// <returns>是否成功解析到候选原点格。</returns>
        public static bool TryResolveSnapTarget(this Puzzle puzzle, Grid grid, out PuzzleGridSnapTarget snapTarget)
        {
            snapTarget = default;
            PuzzleView puzzleView = puzzle?.GetComponent<PuzzleView>();
            GridView gridView = grid?.GetComponent<GridView>();
            if (puzzleView == null || gridView == null || puzzleView.Transform == null || puzzleView.BodyCollider2D == null || gridView.CompositeCollider2D == null)
            {
                return false;
            }

            // 未触碰 Grid 碰撞范围时不产生候选吸附目标
            if (!puzzleView.BodyCollider2D.bounds.Intersects(gridView.CompositeCollider2D.bounds))
            {
                return false;
            }

            // 以 Puzzle 原点格世界坐标反查最近 Grid 坐标作为候选锚点
            Vector3 originWorldPosition = puzzleView.GetOriginWorldPosition();
            Vector2 gridCoordinate = gridView.WorldToGridCoordinate(originWorldPosition);
            int anchorX = Mathf.Clamp(Mathf.RoundToInt(gridCoordinate.x), 0, grid.Width - 1);
            int anchorY = Mathf.Clamp(Mathf.RoundToInt(gridCoordinate.y), 0, grid.Height - 1);
            Slot gridSlot = grid.GetSlot(anchorX, anchorY);
            if (gridSlot == null)
            {
                return false;
            }

            // 保留连续格坐标用于判断候选锚点位于边、角还是中心区域
            snapTarget = new PuzzleGridSnapTarget()
            {
                GridSlot = gridSlot,
                AnchorX = anchorX,
                AnchorY = anchorY,
                Region = ResolvePreviewRegion(grid, gridCoordinate, anchorX, anchorY),
            };
            return true;
        }

        /// <summary>
        /// 尝试按 Develop 项目的外圈 Slot 接近规则解析第一次吸附的候选原点格。
        /// </summary>
        /// <param name="puzzle">当前拖拽中的 Puzzle。</param>
        /// <param name="grid">目标 Grid。</param>
        /// <param name="snapTarget">返回候选吸附目标。</param>
        /// <returns>是否成功解析到候选吸附目标。</returns>
        public static bool TryResolveEntrySnapTarget(this Puzzle puzzle, Grid grid, out PuzzleGridSnapTarget snapTarget)
        {
            snapTarget = default;
            PuzzleView puzzleView = puzzle?.GetComponent<PuzzleView>();
            GridView gridView = grid?.GetComponent<GridView>();
            if (puzzleView == null || gridView == null || puzzleView.BodyCollider2D == null || gridView.CompositeCollider2D == null)
            {
                return false;
            }

            // 只有 Puzzle 主碰撞体触碰 Grid 碰撞范围时才尝试进入吸附
            // 成本低 可以接受此处update检测
            if (!puzzleView.BodyCollider2D.bounds.Intersects(gridView.CompositeCollider2D.bounds))
            {
                return false;
            }

            // 用 Grid 碰撞体最近点作为入口接触参考，寻找双方最接近的外圈 Slot
            Vector3 contactWorldPosition = gridView.CompositeCollider2D.ClosestPoint(puzzleView.Transform.position);
            Slot gridEdgeSlot = grid.GetClosestEdgeSlot(gridView, contactWorldPosition);
            Slot puzzleEdgeSlot = puzzle.GetClosestEntrySlot(puzzleView, contactWorldPosition);
            if (gridEdgeSlot == null || puzzleEdgeSlot == null)
            {
                return false;
            }

            // 根据接触方向和 Puzzle 形状格偏移反推原点锚点
            Vector3 gridSlotWorldPosition = gridView.GetSlotWorldPosition(gridEdgeSlot);
            Vector3 puzzleContactWorldPosition = puzzleView.BodyCollider2D.ClosestPoint(gridSlotWorldPosition);
            GridContactRegion region = ResolveEntryRegion(grid, gridEdgeSlot, gridSlotWorldPosition, puzzleContactWorldPosition);
            GetRegionOffset(region, out int regionOffsetX, out int regionOffsetY);
            
            //考虑旋转
            puzzle.GetRotatedOffset(puzzleEdgeSlot, out int puzzleOffsetX, out int puzzleOffsetY);

            int anchorX = gridEdgeSlot.X + regionOffsetX - puzzleOffsetX;
            int anchorY = gridEdgeSlot.Y - regionOffsetY - puzzleOffsetY;

            snapTarget = new PuzzleGridSnapTarget()
            {
                GridSlot = gridEdgeSlot,
                AnchorX = anchorX,
                AnchorY = anchorY,
                Region = region,
            };
            return true;
        }

        /// <summary>
        /// 将世界坐标转换为 Grid 的连续格坐标。
        /// </summary>
        /// <param name="gridView">目标 GridView。</param>
        /// <param name="worldPosition">要转换的世界坐标。</param>
        /// <returns>连续格坐标，整数表示格中心。</returns>
        public static Vector2 WorldToGridCoordinate(this GridView gridView, Vector3 worldPosition)
        {
            Grid grid = gridView.GetParent<Grid>();
            Vector3 localPosition = gridView.SlotRoot != null ? gridView.SlotRoot.InverseTransformPoint(worldPosition) : gridView.Transform.InverseTransformPoint(worldPosition);
            float coordinateX = localPosition.x / gridView.CellSize + (grid.Width - 1) * 0.5f;
            float coordinateY = (grid.Height - 1) * 0.5f - localPosition.y / gridView.CellSize;
            return new Vector2(coordinateX, coordinateY);
        }


        /// <summary>
        /// 判断当前 Puzzle 主体是否仍与 Grid 的可放置区域保持接触。
        /// </summary>
        /// <param name="puzzle">当前拖拽中的 Puzzle。</param>
        /// <param name="grid">关卡中的唯一 Grid。</param>
        /// <returns>当前是否仍和 Grid 保持接触。</returns>
        public static bool IsTouchingGrid(this Puzzle puzzle, Grid grid)
        {
            PuzzleView puzzleView = puzzle?.GetComponent<PuzzleView>();
            GridView gridView = grid?.GetComponent<GridView>();
            return puzzleView != null
                    && gridView != null
                    && puzzleView.BodyCollider2D != null
                    && gridView.CompositeCollider2D != null
                    && puzzleView.BodyCollider2D.bounds.Intersects(gridView.CompositeCollider2D.bounds);
        }

        /// <summary>
        /// 判断当前 Puzzle 的全部形状格锚点是否都已经落入 Grid 碰撞范围。
        /// </summary>
        /// <param name="puzzle">当前拖拽中的 Puzzle。</param>
        /// <param name="grid">目标 Grid。</param>
        /// <returns>全部形状格是否都在 Grid 内。</returns>
        public static bool IsFullyInsideGrid(this Puzzle puzzle, Grid grid)
        {
            PuzzleView puzzleView = puzzle?.GetComponent<PuzzleView>();
            GridView gridView = grid?.GetComponent<GridView>();
            if (puzzleView == null || gridView == null || gridView.CompositeCollider2D == null || puzzle.ChildrenCount() <= 0)
            {
                return false;
            }

            foreach (Entity child in puzzle.Children.Values)
            {
                Slot puzzleSlot = child as Slot;
                if (puzzleSlot == null)
                {
                    continue;
                }

                Transform slotAnchorTransform = puzzleView.GetSlotAnchorTransform(puzzleSlot);
                if (slotAnchorTransform == null || !gridView.CompositeCollider2D.bounds.Contains(slotAnchorTransform.position))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 按旧实现的矩形范围规则判断指针是否仍处于可吸附拖拽范围。
        /// </summary>
        /// <param name="puzzle">当前拖拽中的 Puzzle。</param>
        /// <param name="grid">目标 Grid。</param>
        /// <param name="gridView">目标 GridView。</param>
        /// <param name="pointerWorldPosition">当前指针世界坐标。</param>
        /// <returns>指针是否仍允许维持吸附模式。</returns>
        public static bool IsPointerInsideAdsorptionRange(this Puzzle puzzle, Grid grid, GridView gridView, Vector3 pointerWorldPosition)
        {
            if (puzzle == null || grid == null || gridView == null)
            {
                return false;
            }

            puzzle.GetRotatedRange(out int minX, out int maxX, out int minY, out int maxY);
            float puzzleWidth = (maxX - minX + 1) * gridView.CellSize;
            float puzzleHeight = (maxY - minY + 1) * gridView.CellSize;
            Vector3 topLeft = gridView.GetGridCoordinateWorldPosition(0, 0);
            Vector3 bottomRight = gridView.GetGridCoordinateWorldPosition(grid.Width - 1, grid.Height - 1);
            Vector3 gridCenter = (topLeft + bottomRight) * 0.5f;
            float halfGridWidth = grid.Width * gridView.CellSize * 0.5f;
            float halfGridHeight = grid.Height * gridView.CellSize * 0.5f;
            return pointerWorldPosition.x >= gridCenter.x - halfGridWidth - puzzleWidth * 0.5f
                    && pointerWorldPosition.x <= gridCenter.x + halfGridWidth + puzzleWidth * 0.5f
                    && pointerWorldPosition.y >= gridCenter.y - halfGridHeight - puzzleHeight * 0.5f
                    && pointerWorldPosition.y <= gridCenter.y + halfGridHeight + puzzleHeight * 0.5f;
        }

        /// <summary>
        /// 根据当前连续格坐标与候选原点格，解析外围吸附应使用的边或角。
        /// </summary>
        /// <param name="grid">目标 Grid。</param>
        /// <param name="gridCoordinate">当前连续格坐标。</param>
        /// <param name="anchorX">候选原点格 X 坐标。</param>
        /// <param name="anchorY">候选原点格 Y 坐标。</param>
        /// <returns>当前外围吸附区域。</returns>
        public static GridContactRegion ResolvePreviewRegion(Grid grid, Vector2 gridCoordinate, int anchorX, int anchorY)
        {
            bool isLeft = anchorX == 0;
            bool isRight = anchorX == grid.Width - 1;
            bool isTop = anchorY == 0;
            bool isBottom = anchorY == grid.Height - 1;
            if ((isLeft || isRight) && (isTop || isBottom))
            {
                return ResolveCornerRegion(gridCoordinate, anchorX, anchorY);
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

            if (isBottom)
            {
                return GridContactRegion.Bottom;
            }

            return GridContactRegion.Center;
        }

        /// <summary>
        /// 将吸附区域转换为世界坐标偏移方向。
        /// </summary>
        /// <param name="region">吸附区域。</param>
        /// <param name="offsetX">返回世界 X 方向偏移。</param>
        /// <param name="offsetY">返回世界 Y 方向偏移。</param>
        public static void GetRegionOffset(GridContactRegion region, out int offsetX, out int offsetY)
        {
            switch (region)
            {
                case GridContactRegion.Left:
                    offsetX = -1;
                    offsetY = 0;
                    return;
                case GridContactRegion.Right:
                    offsetX = 1;
                    offsetY = 0;
                    return;
                case GridContactRegion.Top:
                    offsetX = 0;
                    offsetY = 1;
                    return;
                case GridContactRegion.Bottom:
                    offsetX = 0;
                    offsetY = -1;
                    return;
                case GridContactRegion.LeftTop:
                    offsetX = -1;
                    offsetY = 1;
                    return;
                case GridContactRegion.RightTop:
                    offsetX = 1;
                    offsetY = 1;
                    return;
                case GridContactRegion.LeftBottom:
                    offsetX = -1;
                    offsetY = -1;
                    return;
                case GridContactRegion.RightBottom:
                    offsetX = 1;
                    offsetY = -1;
                    return;
                default:
                    offsetX = 0;
                    offsetY = 0;
                    return;
            }
        }

        /// <summary>
        /// 根据指针相对当前吸附目标的方向解析一步离散移动。
        /// </summary>
        /// <param name="originWorldPosition">当前吸附目标世界坐标。</param>
        /// <param name="pointerWorldPosition">当前指针世界坐标。</param>
        /// <param name="collapseDiagonal">是否把斜向方向折叠为上下左右。</param>
        /// <returns>解析出的离散方向。</returns>
        public static PuzzleSnapDirection ResolveSnapDirection(Vector3 originWorldPosition, Vector3 pointerWorldPosition, bool collapseDiagonal)
        {
            Vector2 relativePoint = new Vector2(pointerWorldPosition.x - originWorldPosition.x, pointerWorldPosition.y - originWorldPosition.y);
            if (relativePoint.sqrMagnitude <= Mathf.Epsilon)
            {
                return PuzzleSnapDirection.None;
            }

            float angle = Mathf.Atan2(relativePoint.y, relativePoint.x) * Mathf.Rad2Deg;
            if (angle < 0f)
            {
                angle += 360f;
            }

            if (angle >= DirectionSectorAngle && angle <= 2f * DirectionSectorAngle)
            {
                return collapseDiagonal ? PuzzleSnapDirection.Right : PuzzleSnapDirection.RightTop;
            }

            if (angle >= 2f * DirectionSectorAngle && angle <= 3f * DirectionSectorAngle)
            {
                return collapseDiagonal ? PuzzleSnapDirection.Up : PuzzleSnapDirection.RightTop;
            }

            if (angle >= 3f * DirectionSectorAngle && angle <= 5f * DirectionSectorAngle)
            {
                return PuzzleSnapDirection.Up;
            }

            if (angle >= 5f * DirectionSectorAngle && angle <= 6f * DirectionSectorAngle)
            {
                return collapseDiagonal ? PuzzleSnapDirection.Up : PuzzleSnapDirection.LeftTop;
            }

            if (angle >= 6f * DirectionSectorAngle && angle <= 7f * DirectionSectorAngle)
            {
                return collapseDiagonal ? PuzzleSnapDirection.Left : PuzzleSnapDirection.LeftTop;
            }

            if (angle >= 7f * DirectionSectorAngle && angle <= 9f * DirectionSectorAngle)
            {
                return PuzzleSnapDirection.Left;
            }

            if (angle >= 9f * DirectionSectorAngle && angle <= 10f * DirectionSectorAngle)
            {
                return collapseDiagonal ? PuzzleSnapDirection.Left : PuzzleSnapDirection.LeftBottom;
            }

            if (angle >= 10f * DirectionSectorAngle && angle <= 11f * DirectionSectorAngle)
            {
                return collapseDiagonal ? PuzzleSnapDirection.Down : PuzzleSnapDirection.LeftBottom;
            }

            if (angle >= 11f * DirectionSectorAngle && angle <= 13f * DirectionSectorAngle)
            {
                return PuzzleSnapDirection.Down;
            }

            if (angle >= 13f * DirectionSectorAngle && angle <= 14f * DirectionSectorAngle)
            {
                return collapseDiagonal ? PuzzleSnapDirection.Down : PuzzleSnapDirection.RightBottom;
            }

            if (angle >= 14f * DirectionSectorAngle && angle <= 15f * DirectionSectorAngle)
            {
                return collapseDiagonal ? PuzzleSnapDirection.Right : PuzzleSnapDirection.RightBottom;
            }

            return PuzzleSnapDirection.Right;
        }

        /// <summary>
        /// 将离散方向转换成 Grid 坐标步进。
        /// </summary>
        /// <param name="direction">离散方向。</param>
        /// <param name="deltaX">返回 X 方向步进。</param>
        /// <param name="deltaY">返回 Y 方向步进。</param>
        public static void GetDirectionDelta(PuzzleSnapDirection direction, out int deltaX, out int deltaY)
        {
            switch (direction)
            {
                case PuzzleSnapDirection.Up:
                    deltaX = 0;
                    deltaY = -1;
                    return;
                case PuzzleSnapDirection.Down:
                    deltaX = 0;
                    deltaY = 1;
                    return;
                case PuzzleSnapDirection.Left:
                    deltaX = -1;
                    deltaY = 0;
                    return;
                case PuzzleSnapDirection.Right:
                    deltaX = 1;
                    deltaY = 0;
                    return;
                case PuzzleSnapDirection.LeftTop:
                    deltaX = -1;
                    deltaY = -1;
                    return;
                case PuzzleSnapDirection.RightTop:
                    deltaX = 1;
                    deltaY = -1;
                    return;
                case PuzzleSnapDirection.LeftBottom:
                    deltaX = -1;
                    deltaY = 1;
                    return;
                case PuzzleSnapDirection.RightBottom:
                    deltaX = 1;
                    deltaY = 1;
                    return;
                default:
                    deltaX = 0;
                    deltaY = 0;
                    return;
            }
        }

        /// <summary>
        /// 获取距离接触点最近的 Grid 外圈 Slot。
        /// </summary>
        /// <param name="grid">目标 Grid。</param>
        /// <param name="gridView">目标 GridView。</param>
        /// <param name="contactWorldPosition">接触点世界坐标。</param>
        /// <returns>最近的外圈 Slot。</returns>
        private static Slot GetClosestEdgeSlot(this Grid grid, GridView gridView, Vector3 contactWorldPosition)
        {
            Slot closestSlot = null;
            float closestDistance = float.MaxValue;
            if (grid == null || gridView == null || grid.ChildrenCount() <= 0)
            {
                return null;
            }

            foreach (Entity child in grid.Children.Values)
            {
                Slot slot = child as Slot;
                if (slot == null || !grid.IsEdgeSlot(slot))
                {
                    continue;
                }

                float distance = Vector2.Distance(contactWorldPosition, gridView.GetSlotWorldPosition(slot));
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestSlot = slot;
                }
            }

            return closestSlot;
        }

        /// <summary>
        /// 获取距离接触点最近的 Puzzle 外圈形状格。
        /// </summary>
        /// <param name="puzzle">目标 Puzzle。</param>
        /// <param name="puzzleView">目标 PuzzleView。</param>
        /// <param name="contactWorldPosition">接触点世界坐标。</param>
        /// <returns>最近的形状格。</returns>
        private static Slot GetClosestEntrySlot(this Puzzle puzzle, PuzzleView puzzleView, Vector3 contactWorldPosition)
        {
            Slot closestSlot = null;
            float closestDistance = float.MaxValue;
            if (puzzle == null || puzzleView == null || puzzle.ChildrenCount() <= 0)
            {
                return null;
            }

            foreach (Entity child in puzzle.Children.Values)
            {
                Slot slot = child as Slot;
                if (slot == null || !puzzle.IsEntrySlot(slot))
                {
                    continue;
                }

                Transform slotAnchorTransform = puzzleView.GetSlotAnchorTransform(slot);
                if (slotAnchorTransform == null)
                {
                    continue;
                }

                float distance = Vector2.Distance(contactWorldPosition, slotAnchorTransform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestSlot = slot;
                }
            }

            return closestSlot;
        }

        /// <summary>
        /// 判断目标 Grid Slot 是否位于外圈。
        /// </summary>
        /// <param name="grid">目标 Grid。</param>
        /// <param name="slot">要检查的 Slot。</param>
        /// <returns>是否为外圈 Slot。</returns>
        private static bool IsEdgeSlot(this Grid grid, Slot slot)
        {
            return slot != null && (slot.X == 0 || slot.Y == 0 || slot.X == grid.Width - 1 || slot.Y == grid.Height - 1);
        }

        /// <summary>
        /// 判断目标 Puzzle Slot 是否作为进入吸附时的接触候选。
        /// </summary>
        /// <param name="puzzle">目标 Puzzle。</param>
        /// <param name="slot">要检查的 Slot。</param>
        /// <returns>是否为接触候选 Slot。</returns>
        private static bool IsEntrySlot(this Puzzle puzzle, Slot slot)
        {
            return slot != null && (slot.X == 0 || slot.Y == 0);
        }

        /// <summary>
        /// 计算当前 Puzzle 旋转后的形状包围范围。
        /// </summary>
        /// <param name="puzzle">目标 Puzzle。</param>
        /// <param name="minX">返回最小 X。</param>
        /// <param name="maxX">返回最大 X。</param>
        /// <param name="minY">返回最小 Y。</param>
        /// <param name="maxY">返回最大 Y。</param>
        private static void GetRotatedRange(this Puzzle puzzle, out int minX, out int maxX, out int minY, out int maxY)
        {
            minX = int.MaxValue;
            maxX = int.MinValue;
            minY = int.MaxValue;
            maxY = int.MinValue;
            foreach (Entity child in puzzle.Children.Values)
            {
                Slot slot = child as Slot;
                if (slot == null)
                {
                    continue;
                }

                puzzle.GetRotatedOffset(slot, out int offsetX, out int offsetY);
                minX = Mathf.Min(minX, offsetX);
                maxX = Mathf.Max(maxX, offsetX);
                minY = Mathf.Min(minY, offsetY);
                maxY = Mathf.Max(maxY, offsetY);
            }

            if (minX == int.MaxValue)
            {
                minX = 0;
                maxX = 0;
                minY = 0;
                maxY = 0;
            }
        }

        /// <summary>
        /// 按 Grid 外圈 Slot 和接触点解析初次吸附停靠区域。
        /// </summary>
        /// <param name="grid">目标 Grid。</param>
        /// <param name="gridSlot">当前接触的 Grid 外圈 Slot。</param>
        /// <param name="gridSlotWorldPosition">Grid Slot 世界坐标。</param>
        /// <param name="contactWorldPosition">Puzzle 碰撞体接触点。</param>
        /// <returns>解析出的吸附区域。</returns>
        private static GridContactRegion ResolveEntryRegion(Grid grid, Slot gridSlot, Vector3 gridSlotWorldPosition, Vector3 contactWorldPosition)
        {
            bool isCorner = (gridSlot.X == 0 || gridSlot.X == grid.Width - 1) && (gridSlot.Y == 0 || gridSlot.Y == grid.Height - 1);
            PuzzleSnapDirection direction = ResolveSnapDirection(gridSlotWorldPosition, contactWorldPosition, !isCorner);
            return DirectionToRegion(grid, gridSlot, direction);
        }

        /// <summary>
        /// 将接触方向转换为吸附区域，并在角落限制为允许的外侧方向。
        /// </summary>
        /// <param name="grid">目标 Grid。</param>
        /// <param name="gridSlot">接触的 Grid Slot。</param>
        /// <param name="direction">接触方向。</param>
        /// <returns>吸附区域。</returns>
        private static GridContactRegion DirectionToRegion(Grid grid, Slot gridSlot, PuzzleSnapDirection direction)
        {
            bool isLeft = gridSlot.X == 0;
            bool isRight = gridSlot.X == grid.Width - 1;
            bool isTop = gridSlot.Y == 0;
            bool isBottom = gridSlot.Y == grid.Height - 1;

            if (isLeft && isTop && (direction == PuzzleSnapDirection.LeftTop || direction == PuzzleSnapDirection.Left || direction == PuzzleSnapDirection.Up))
            {
                return direction == PuzzleSnapDirection.Left ? GridContactRegion.Left : direction == PuzzleSnapDirection.Up ? GridContactRegion.Top : GridContactRegion.LeftTop;
            }

            if (isRight && isTop && (direction == PuzzleSnapDirection.RightTop || direction == PuzzleSnapDirection.Right || direction == PuzzleSnapDirection.Up))
            {
                return direction == PuzzleSnapDirection.Right ? GridContactRegion.Right : direction == PuzzleSnapDirection.Up ? GridContactRegion.Top : GridContactRegion.RightTop;
            }

            if (isLeft && isBottom && (direction == PuzzleSnapDirection.LeftBottom || direction == PuzzleSnapDirection.Left || direction == PuzzleSnapDirection.Down))
            {
                return direction == PuzzleSnapDirection.Left ? GridContactRegion.Left : direction == PuzzleSnapDirection.Down ? GridContactRegion.Bottom : GridContactRegion.LeftBottom;
            }

            if (isRight && isBottom && (direction == PuzzleSnapDirection.RightBottom || direction == PuzzleSnapDirection.Right || direction == PuzzleSnapDirection.Down))
            {
                return direction == PuzzleSnapDirection.Right ? GridContactRegion.Right : direction == PuzzleSnapDirection.Down ? GridContactRegion.Bottom : GridContactRegion.RightBottom;
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

            if (isBottom)
            {
                return GridContactRegion.Bottom;
            }

            return GridContactRegion.Center;
        }

        /// <summary>
        /// 根据角落格内部的 1/3 分区和边缘距离解析最终角落区域。
        /// </summary>
        /// <param name="gridCoordinate">当前连续格坐标。</param>
        /// <param name="anchorX">候选角落格 X 坐标。</param>
        /// <param name="anchorY">候选角落格 Y 坐标。</param>
        /// <returns>当前角落接触区域。</returns>
        private static GridContactRegion ResolveCornerRegion(Vector2 gridCoordinate, int anchorX, int anchorY)
        {
            float localX = Mathf.Clamp01(gridCoordinate.x - anchorX + 0.5f);
            float localY = Mathf.Clamp01(gridCoordinate.y - anchorY + 0.5f);

            if (anchorX == 0 && anchorY == 0)
            {
                if (localX <= CornerThirdThreshold || localY <= CornerThirdThreshold)
                {
                    return GridContactRegion.LeftTop;
                }

                return localX <= localY ? GridContactRegion.Left : GridContactRegion.Top;
            }

            if (anchorX > 0 && anchorY == 0)
            {
                if (localX >= 1f - CornerThirdThreshold || localY <= CornerThirdThreshold)
                {
                    return GridContactRegion.RightTop;
                }

                return 1f - localX <= localY ? GridContactRegion.Right : GridContactRegion.Top;
            }

            if (anchorX == 0)
            {
                if (localX <= CornerThirdThreshold || localY >= 1f - CornerThirdThreshold)
                {
                    return GridContactRegion.LeftBottom;
                }

                return localX <= 1f - localY ? GridContactRegion.Left : GridContactRegion.Bottom;
            }

            if (localX >= 1f - CornerThirdThreshold || localY >= 1f - CornerThirdThreshold)
            {
                return GridContactRegion.RightBottom;
            }

            return 1f - localX <= 1f - localY ? GridContactRegion.Right : GridContactRegion.Bottom;
        }
    }
}
