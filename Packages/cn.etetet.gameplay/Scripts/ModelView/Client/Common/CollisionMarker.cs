using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 通用二维碰撞标记组件，用于在运行时区分不同碰撞对象的语义分组。
    /// </summary>
    [EnableClass]
    public class CollisionMarker : MonoBehaviour
    {
        /// <summary>
        /// 当前碰撞对象所属的语义分组。
        /// </summary>
        public string Group;

        /// <summary>
        /// 当前对象是否为该语义分组下的主碰撞体。
        /// </summary>
        public bool Primary;
    }
}
