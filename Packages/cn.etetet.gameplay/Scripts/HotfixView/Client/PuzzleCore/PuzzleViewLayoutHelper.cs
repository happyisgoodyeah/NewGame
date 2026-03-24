using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 负责计算 PuzzleCore 默认表现层的场景布局。
    /// </summary>
    public static class PuzzleViewLayoutHelper
    {
        /// <summary>
        /// 获取默认 Grid 的世界坐标。
        /// </summary>
        public static Vector3 GetGridWorldPosition()
        {
            return Vector3.zero;
        }

        /// <summary>
        /// 计算指定 Puzzle 的默认初始世界坐标。
        /// </summary>
        public static Vector3 GetPuzzleWorldPosition(Grid grid, Puzzle puzzle)
        {
            if (puzzle.Id == PuzzleCoreConst.DefaultLeftPuzzleId)
            {
                return GetLeftPuzzleWorldPosition(grid);
            }

            return GetRightPuzzleWorldPosition(grid);
        }

        /// <summary>
        /// 计算左侧 Puzzle 的初始世界坐标。
        /// </summary>
        private static Vector3 GetLeftPuzzleWorldPosition(Grid grid)
        {
            float halfGridWidth = grid.Width * PuzzleViewConst.GridCellSize * 0.5f;
            float x = -halfGridWidth - PuzzleViewConst.PuzzleHorizontalMargin;
            return new Vector3(x, PuzzleViewConst.LeftPuzzleStartY, 0f);
        }

        /// <summary>
        /// 计算右侧 Puzzle 的初始世界坐标。
        /// </summary>
        private static Vector3 GetRightPuzzleWorldPosition(Grid grid)
        {
            float halfGridWidth = grid.Width * PuzzleViewConst.GridCellSize * 0.5f;
            float x = halfGridWidth + PuzzleViewConst.PuzzleHorizontalMargin;
            return new Vector3(x, PuzzleViewConst.RightPuzzleStartY, 0f);
        }
    }
}
