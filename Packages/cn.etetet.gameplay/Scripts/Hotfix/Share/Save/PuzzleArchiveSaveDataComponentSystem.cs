using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 拼图图鉴存档数据维护逻辑
    /// </summary>
    [EntitySystemOf(typeof(PuzzleArchiveSaveDataComponent))]
    public static partial class PuzzleArchiveSaveDataComponentSystem
    {
        /// <summary>
        /// 初始化拼图图鉴存档数据
        /// </summary>
        [EntitySystem]
        private static void Awake(this PuzzleArchiveSaveDataComponent self)
        {
            self.DataVersion = SaveConst.CurrentDataVersion;
            self.UnlockedPuzzleIds = new List<int>();
            self.LastModifiedTime = DateTime.Now;
        }

        /// <summary>
        /// 判断拼图是否已经解锁
        /// </summary>
        /// <param name="self">拼图图鉴存档数据</param>
        /// <param name="puzzleConfigId">Puzzle 配置 id</param>
        /// <returns>是否已解锁</returns>
        public static bool IsPuzzleUnlocked(this PuzzleArchiveSaveDataComponent self, int puzzleConfigId)
        {
            return self.UnlockedPuzzleIds.Contains(puzzleConfigId);
        }

        /// <summary>
        /// 解锁指定拼图
        /// </summary>
        /// <param name="self">拼图图鉴存档数据</param>
        /// <param name="puzzleConfigId">Puzzle 配置 id</param>
        /// <returns>本次调用是否新增解锁记录</returns>
        public static bool UnlockPuzzle(this PuzzleArchiveSaveDataComponent self, int puzzleConfigId)
        {
            if (puzzleConfigId <= 0 || self.UnlockedPuzzleIds.Contains(puzzleConfigId))
            {
                return false;
            }

            self.UnlockedPuzzleIds.Add(puzzleConfigId);
            self.LastModifiedTime = DateTime.Now;
            return true;
        }

        /// <summary>
        /// 批量解锁指定拼图
        /// </summary>
        /// <param name="self">拼图图鉴存档数据</param>
        /// <param name="puzzleConfigIds">Puzzle 配置 id 集合</param>
        /// <returns>本次调用是否新增任意解锁记录</returns>
        public static bool UnlockPuzzles(this PuzzleArchiveSaveDataComponent self, IReadOnlyList<int> puzzleConfigIds)
        {
            bool changed = false;
            for (int i = 0; i < puzzleConfigIds.Count; ++i)
            {
                changed |= self.UnlockPuzzle(puzzleConfigIds[i]);
            }

            return changed;
        }

        /// <summary>
        /// 解锁指定关卡内配置的全部拼图
        /// </summary>
        /// <param name="self">拼图图鉴存档数据</param>
        /// <param name="gridConfigId">Grid 配置 id</param>
        /// <returns>本次调用是否新增任意解锁记录</returns>
        public static bool UnlockPuzzlesByLevel(this PuzzleArchiveSaveDataComponent self, int gridConfigId)
        {
            GridConfig gridConfig = GridConfigCategory.Instance.Get(gridConfigId);
            if (gridConfig.PuzzleList == null)
            {
                throw new Exception($"grid config puzzle list is null: id={gridConfigId}");
            }

            bool changed = false;
            for (int i = 0; i < gridConfig.PuzzleList.Count; ++i)
            {
                changed |= self.UnlockPuzzle(gridConfig.PuzzleList[i].Id);
            }

            return changed;
        }
    }
}
