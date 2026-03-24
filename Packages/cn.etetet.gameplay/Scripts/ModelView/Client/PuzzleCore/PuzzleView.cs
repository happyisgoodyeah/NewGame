using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// Puzzle 的场景表现组件，负责拼图主图片、碰撞体和预制体内结构节点绑定。
    /// </summary>
    [ComponentOf(typeof(Puzzle))]
    public class PuzzleView : Entity, IAwake<string, Vector3>, IDestroy
    {
        /// <summary>
        /// Puzzle 在场景中的根节点对象。
        /// </summary>
        public GameObject GameObject { get; set; }

        /// <summary>
        /// Puzzle 根节点的 Transform。
        /// </summary>
        public Transform Transform { get; set; }

        /// <summary>
        /// Puzzle 主图使用的 SpriteRenderer。
        /// </summary>
        public SpriteRenderer SpriteRenderer { get; set; }

        /// <summary>
        /// 用于和 Grid Slot 接触检测的多边形碰撞体。
        /// </summary>
        public PolygonCollider2D PolygonCollider2D { get; set; }

        /// <summary>
        /// 用于和 Grid 区域接触检测的主体碰撞体。
        /// </summary>
        public Collider2D BodyCollider2D { get; set; }

        /// <summary>
        /// Puzzle 预制体上的 Rigidbody2D。
        /// </summary>
        public Rigidbody2D Rigidbody2D { get; set; }

        /// <summary>
        /// 当前默认 1x1 Puzzle 对应的 Slot 锚点。
        /// </summary>
        public Transform SlotAnchorTransform { get; set; }

        /// <summary>
        /// 当前 Puzzle 表现层采用的移动模式。
        /// </summary>
        public PuzzleMoveMode MoveMode { get; set; }
    }
}
