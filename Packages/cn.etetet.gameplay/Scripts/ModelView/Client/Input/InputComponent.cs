using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 场景级通用输入组件，负责维护指针按压状态与基础输入配置。
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class InputComponent : Entity, IAwake, IUpdate, IDestroy
    {
        /// <summary>
        /// 当前用于屏幕坐标转世界坐标的主相机。
        /// </summary>
        public Camera MainCamera { get; set; }

        /// <summary>
        /// 指针命中检测使用的 LayerMask。
        /// </summary>
        public int PhysicsLayerMask { get; set; }

        /// <summary>
        /// 当前是否处于按压状态。
        /// </summary>
        public bool IsPointerPressed { get; set; }

        /// <summary>
        /// 本次按压开始时的屏幕坐标。
        /// </summary>
        public Vector2 PressScreenPosition { get; set; }

        /// <summary>
        /// 本次按压开始时的世界坐标。
        /// </summary>
        public Vector3 PressWorldPosition { get; set; }

        /// <summary>
        /// 本次按压开始时的未缩放时间。
        /// </summary>
        public float PressStartTime { get; set; }
    }
}
