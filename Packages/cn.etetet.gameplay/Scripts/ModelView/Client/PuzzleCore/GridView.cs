using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// Grid 的场景表现组件，负责提供棋盘根节点、拼图容器和 Grid Slot 的挂载容器。
    /// </summary>
    [ComponentOf(typeof(Grid))]
    public class GridView : Entity, IAwake<Vector3, float>, IDestroy
    {
        /// <summary>
        /// Grid 在场景中的根节点对象。
        /// </summary>
        public GameObject GameObject { get; set; }

        /// <summary>
        /// Grid 根节点的 Transform。
        /// </summary>
        public Transform Transform { get; set; }

        /// <summary>
        /// Grid 下所有 Grid SlotView 的挂载根节点。
        /// </summary>
        public Transform SlotRoot { get; set; }

        /// <summary>
        /// Grid 下用于挂载已进入棋盘区域拼图表现的根节点。
        /// </summary>
        public Transform PuzzleRoot { get; set; }

        /// <summary>
        /// Grid 区域整体碰撞检测使用的 CompositeCollider2D。
        /// </summary>
        public CompositeCollider2D CompositeCollider2D { get; set; }

        /// <summary>
        /// 棋盘中相邻两个 Slot 的世界间距。
        /// </summary>
        public float CellSize { get; set; }
    }
}
