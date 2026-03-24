namespace ET.Client
{
    /// <summary>
    /// 负责在合适的场景时机重新发布 PuzzleCore 的表现层创建事件。
    /// </summary>
    public static class PuzzleViewEventHelper
    {
        /// <summary>
        /// 判断当前 ET CurrentScene 对应的 Unity 场景是否已经可用于创建表现层。
        /// </summary>
        public static bool IsSceneReady(Scene etScene)
        {
            return etScene.TryGetUnityScene(out _);
        }

        /// <summary>
        /// 在场景真正加载完成后，为现有数据重新发布 View 创建事件。
        /// </summary>
        public static void PublishCreateViewEvents(Scene etScene)
        {
            etScene.EnsureInitialized();

            Grid grid = etScene.GetGrid();
            if (grid != null)
            {
                EventSystem.Instance.Publish(etScene, new AfterCreateGrid() { Grid = grid });
                PublishSlotCreateEvents(etScene, grid);
            }

            if (etScene.ChildrenCount() == 0)
            {
                return;
            }

            foreach (Entity child in etScene.Children.Values)
            {
                Puzzle puzzle = child as Puzzle;
                if (puzzle == null)
                {
                    continue;
                }

                EventSystem.Instance.Publish(etScene, new AfterCreatePuzzle() { Puzzle = puzzle });
                PublishSlotCreateEvents(etScene, puzzle);
            }
        }

        /// <summary>
        /// 为指定父实体下已有的所有 Slot 重新发布 SlotView 创建事件。
        /// </summary>
        private static void PublishSlotCreateEvents(Scene etScene, Entity parent)
        {
            if (parent.ChildrenCount() == 0)
            {
                return;
            }

            foreach (Entity child in parent.Children.Values)
            {
                Slot slot = child as Slot;
                if (slot == null)
                {
                    continue;
                }

                EventSystem.Instance.Publish(etScene, new AfterCreateSlot() { Slot = slot });
            }
        }
    }
}
