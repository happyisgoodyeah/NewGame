namespace ET.Client
{
    /// <summary>
    /// GridSlotStateComponent 的状态维护逻辑。
    /// </summary>
    [EntitySystemOf(typeof(GridSlotStateComponent))]
    public static partial class GridSlotStateComponentSystem
    {
        /// <summary>
        /// 初始化当前 Grid Slot 的放置状态。
        /// </summary>
        [EntitySystem]
        private static void Awake(this GridSlotStateComponent self)
        {
        }

        /// <summary>
        /// 清理当前 Grid Slot 的放置状态。
        /// </summary>
        [EntitySystem]
        private static void Destroy(this GridSlotStateComponent self)
        {
        }

        /// <summary>
        /// 判断当前 Grid Slot 是否允许放置新的拼图。
        /// </summary>
        /// <param name="self">要检查的 Grid Slot 状态组件。</param>
        /// <returns>当前格子是否允许放置。</returns>
        public static bool CanPlacePuzzle(this GridSlotStateComponent self)
        {
            if (self == null)
            {
                return false;
            }

            Slot slot = self.GetParent<Slot>();
            if (slot == null || slot.Parent is not Grid)
            {
                return false;
            }

            Puzzle bindingPuzzle = slot.BindingPuzzle;
            return slot.Kind == SlotType.GridPlaceable && bindingPuzzle == null;
        }
    }
}
