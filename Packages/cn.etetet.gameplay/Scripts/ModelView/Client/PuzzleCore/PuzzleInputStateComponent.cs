using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// PuzzleCore 输入状态组件，负责记录当前场景里的按压候选与拖拽对象。
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class PuzzleInputStateComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 当前正在被拖拽的 Puzzle。
        /// </summary>
        public EntityRef<Puzzle> DraggingPuzzle { get; set; }

        /// <summary>
        /// 当前按下后仍处于待确认状态的 Puzzle。
        /// </summary>
        public EntityRef<Puzzle> PendingPressPuzzle { get; set; }

        /// <summary>
        /// 本次待确认按压开始时的屏幕坐标。
        /// </summary>
        public Vector2 PendingPressScreenPosition { get; set; }

        /// <summary>
        /// 本次待确认按压开始时的世界坐标。
        /// </summary>
        public Vector3 PendingPressWorldPosition { get; set; }
    }
}
