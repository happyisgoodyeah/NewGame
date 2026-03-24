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
    /// 一次 Grid 接触解析得到的候选吸附目标。
    /// </summary>
    public struct PuzzleGridSnapTarget
    {
        /// <summary>
        /// 候选原点格。
        /// </summary>
        public Slot GridSlot;

        /// <summary>
        /// 候选原点格的 X 坐标。
        /// </summary>
        public int AnchorX;

        /// <summary>
        /// 候选原点格的 Y 坐标。
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
        private const float CornerThirdThreshold = 1f / 3f;

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
            if (puzzleView == null || gridView == null || puzzleView.BodyCollider2D == null || gridView.CompositeCollider2D == null)
            {
                return false;
            }

            if (!puzzleView.BodyCollider2D.bounds.Intersects(gridView.CompositeCollider2D.bounds))
            {
                return false;
            }

            Vector3 originWorldPosition = puzzleView.GetOriginWorldPosition();
            Vector2 gridCoordinate = gridView.WorldToGridCoordinate(originWorldPosition);
            int anchorX = Mathf.Clamp(Mathf.RoundToInt(gridCoordinate.x), 0, grid.Width - 1);
            int anchorY = Mathf.Clamp(Mathf.RoundToInt(gridCoordinate.y), 0, grid.Height - 1);
            Slot gridSlot = grid.GetSlot(anchorX, anchorY);
            if (gridSlot == null)
            {
                return false;
            }

            snapTarget = new PuzzleGridSnapTarget()
            {
                GridSlot = gridSlot,
                AnchorX = anchorX,
                AnchorY = anchorY,
                Region = ResolveContactRegion(grid, gridCoordinate, anchorX, anchorY),
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
        /// 获取 Puzzle 当前原点 Slot 的世界坐标。
        /// </summary>
        /// <param name="puzzleView">当前 PuzzleView。</param>
        /// <returns>原点 Slot 的世界坐标。</returns>
        public static Vector3 GetOriginWorldPosition(this PuzzleView puzzleView)
        {
            if (puzzleView.SlotAnchorTransform != null)
            {
                return puzzleView.SlotAnchorTransform.position;
            }

            return puzzleView.Transform != null ? puzzleView.Transform.position : Vector3.zero;
        }

        /// <summary>
        /// 根据当前连续格坐标与边界格位置解析边/角吸附区域。
        /// </summary>
        /// <param name="grid">目标 Grid。</param>
        /// <param name="gridCoordinate">连续格坐标。</param>
        /// <param name="anchorX">候选原点格 X 坐标。</param>
        /// <param name="anchorY">候选原点格 Y 坐标。</param>
        /// <returns>当前接触区域类型。</returns>
        private static GridContactRegion ResolveContactRegion(Grid grid, Vector2 gridCoordinate, int anchorX, int anchorY)
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
                if (localX <= CornerThirdThreshold && localY <= CornerThirdThreshold)
                {
                    return GridContactRegion.LeftTop;
                }

                return localX <= localY ? GridContactRegion.Left : GridContactRegion.Top;
            }

            if (anchorX > 0 && anchorY == 0)
            {
                if (localX >= 1f - CornerThirdThreshold && localY <= CornerThirdThreshold)
                {
                    return GridContactRegion.RightTop;
                }

                return 1f - localX <= localY ? GridContactRegion.Right : GridContactRegion.Top;
            }

            if (anchorX == 0)
            {
                if (localX <= CornerThirdThreshold && localY >= 1f - CornerThirdThreshold)
                {
                    return GridContactRegion.LeftBottom;
                }

                return localX <= 1f - localY ? GridContactRegion.Left : GridContactRegion.Bottom;
            }

            if (localX >= 1f - CornerThirdThreshold && localY >= 1f - CornerThirdThreshold)
            {
                return GridContactRegion.RightBottom;
            }

            return 1f - localX <= 1f - localY ? GridContactRegion.Right : GridContactRegion.Bottom;
        }
    }
}
