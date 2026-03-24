namespace ET.Client
{
    /// <summary>
    /// PuzzleCore 输入状态组件，负责记录当前场景里被拖拽的 Puzzle。
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class PuzzleInputStateComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 当前正在被拖拽的 Puzzle。
        /// </summary>
        public EntityRef<Puzzle> DraggingPuzzle { get; set; }
    }
}
