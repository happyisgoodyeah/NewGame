using DG.Tweening;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// Puzzle 的拖拽状态组件，负责保存当前拖拽过程中的运行时数据。
    /// </summary>
    [ComponentOf(typeof(Puzzle))]
    public class PuzzleDragComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 当前 Puzzle 是否正在被拖拽。
        /// </summary>
        public bool IsDragging { get; set; }

        /// <summary>
        /// 本次拖拽开始时 Puzzle 的世界坐标。
        /// </summary>
        public Vector3 DragStartWorldPosition { get; set; }

        /// <summary>
        /// Puzzle 初始生成时的世界坐标。
        /// </summary>
        public Vector3 InitialWorldPosition { get; set; }

        /// <summary>
        /// 是否已经缓存过 Puzzle 初始生成时的世界坐标。
        /// </summary>
        public bool HasInitialWorldPosition { get; set; }

        /// <summary>
        /// 本次拖拽开始前 Puzzle 的状态。
        /// </summary>
        public PuzzleState DragStartState { get; set; }

        /// <summary>
        /// 本次拖拽开始前 Puzzle 原点格的 X 坐标。
        /// </summary>
        public int DragStartAnchorX { get; set; }

        /// <summary>
        /// 本次拖拽开始前 Puzzle 原点格的 Y 坐标。
        /// </summary>
        public int DragStartAnchorY { get; set; }

        /// <summary>
        /// 最近一次拖拽更新时的指针世界坐标。
        /// </summary>
        public Vector3 LastPointerWorldPosition { get; set; }

        /// <summary>
        /// 当前用于驱动拼图平滑移动的 Tween。
        /// </summary>
        public Tween MoveTween { get; set; }

        /// <summary>
        /// 当前 GridSnap 模式下原点格的 X 坐标。
        /// </summary>
        public int SnapAnchorX { get; set; }

        /// <summary>
        /// 当前 GridSnap 模式下原点格的 Y 坐标。
        /// </summary>
        public int SnapAnchorY { get; set; }

        /// <summary>
        /// 当前拖拽过程中是否已经进入 GridSnap 模式。
        /// </summary>
        public bool IsGridSnapActive { get; set; }
    }
}
