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
        /// 获取指定 Puzzle 的初始世界坐标。
        /// </summary>
        /// <param name="puzzle">目标 Puzzle。</param>
        /// <returns>当前 Puzzle 配置记录的初始世界坐标。</returns>
        public static Vector3 GetPuzzleWorldPosition(Puzzle puzzle)
        {
            if (puzzle == null)
            {
                return Vector3.zero;
            }

            return new Vector3(puzzle.InitialWorldPositionX, puzzle.InitialWorldPositionY, puzzle.InitialWorldPositionZ);
        }
    }
}
