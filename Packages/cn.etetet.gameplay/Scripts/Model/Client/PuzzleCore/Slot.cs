namespace ET.Client
{
    /// <summary>
    /// Grid 和 Puzzle 共同复用的最小匹配单位。
    /// </summary>
    [ChildOf]
    public class Slot : Entity, IAwake<int, int, SlotType>
    {
        /// <summary>
        /// Slot 在父节点局部坐标系中的 X 坐标。
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Slot 在父节点局部坐标系中的 Y 坐标。
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// 当前 Slot 的用途类型。
        /// </summary>
        public SlotType Kind { get; set; }

        /// <summary>
        /// 当前绑定到该 Grid Slot 的 Puzzle 引用，仅对 Grid 下的可放置格子有意义。
        /// </summary>
        public EntityRef<Puzzle> BindingPuzzle { get; set; }
    }
}
