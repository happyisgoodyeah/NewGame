using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// Slot 的场景表现组件，负责绑定或生成当前 Slot 对应的场景节点。
    /// </summary>
    [ComponentOf(typeof(Slot))]
    public class SlotView : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 当前 Slot 绑定的场景对象。
        /// </summary>
        public GameObject GameObject { get; set; }

        /// <summary>
        /// 当前 Slot 绑定对象的 Transform。
        /// </summary>
        public Transform Transform { get; set; }

        /// <summary>
        /// 当前 Slot 绑定对象上的二维碰撞体。
        /// </summary>
        public Collider2D Collider2D { get; set; }

        /// <summary>
        /// 当前 Slot 对应的碰撞标记组件。
        /// </summary>
        public CollisionMarker CollisionMarker { get; set; }

        /// <summary>
        /// 当前绑定对象是否由 SlotView 在运行时实例化。
        /// </summary>
        public bool OwnsGameObject { get; set; }
    }
}
