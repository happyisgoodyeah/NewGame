using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 当前输入对应的按钮类型。
    /// </summary>
    public enum PointerButton : byte
    {
        /// <summary>
        /// 鼠标左键或主指针主按钮。
        /// </summary>
        Left = 0,
    }

    /// <summary>
    /// 一次输入命中结果中的单个对象信息。
    /// </summary>
    public struct InputHitResult
    {
        /// <summary>
        /// 本次命中的二维碰撞体。
        /// </summary>
        public Collider2D Collider2D;

        /// <summary>
        /// 命中碰撞体所在的 GameObject。
        /// </summary>
        public GameObject GameObject;

        /// <summary>
        /// 命中碰撞体所在的 Transform。
        /// </summary>
        public Transform Transform;

        /// <summary>
        /// 命中对象当前的 Unity Tag。
        /// </summary>
        public string Tag;

        /// <summary>
        /// 通过 GameObjectEntityRef 反查到的 ET Entity。
        /// </summary>
        public Entity Entity;
    }

    /// <summary>
    /// 通用指针输入上下文，输入层负责采集基础信息并携带手势判定所需状态
    /// </summary>
    public struct PointerInputContext
    {
        /// <summary>
        /// 本次输入对应的按钮类型。
        /// </summary>
        public PointerButton Button;

        /// <summary>
        /// 当前帧指针所在的屏幕坐标。
        /// </summary>
        public Vector2 ScreenPosition;

        /// <summary>
        /// 当前帧指针所在的世界坐标。
        /// </summary>
        public Vector3 WorldPosition;

        /// <summary>
        /// 本次按压起始时的屏幕坐标。
        /// </summary>
        public Vector2 PressScreenPosition;

        /// <summary>
        /// 本次按压起始时的世界坐标。
        /// </summary>
        public Vector3 PressWorldPosition;

        /// <summary>
        /// 当前按压已持续的时长，单位秒。
        /// </summary>
        public float Duration;

        /// <summary>
        /// 当前指针位置命中的全部结果数组。
        /// </summary>
        public InputHitResult[] HitResults;

        /// <summary>
        /// 本次按压开始时命中的全部结果数组，适合拖拽和点击使用固定起始目标
        /// </summary>
        public InputHitResult[] PressHitResults;
    }

    /// <summary>
    /// 指针按下事件。
    /// </summary>
    public struct PointerDown
    {
        /// <summary>
        /// 本次事件的输入上下文。
        /// </summary>
        public PointerInputContext Context;
    }

    /// <summary>
    /// 指针按住事件。
    /// </summary>
    public struct PointerHold
    {
        /// <summary>
        /// 本次事件的输入上下文。
        /// </summary>
        public PointerInputContext Context;
    }

    /// <summary>
    /// 指针松开事件。
    /// </summary>
    public struct PointerUp
    {
        /// <summary>
        /// 本次事件的输入上下文。
        /// </summary>
        public PointerInputContext Context;
    }

    /// <summary>
    /// 指针点击事件。
    /// </summary>
    public struct PointerClick
    {
        /// <summary>
        /// 本次事件的输入上下文。
        /// </summary>
        public PointerInputContext Context;
    }

    /// <summary>
    /// 指针拖拽开始事件
    /// </summary>
    public struct PointerDragStart
    {
        /// <summary>
        /// 本次事件的输入上下文。
        /// </summary>
        public PointerInputContext Context;
    }

    /// <summary>
    /// 指针拖拽持续事件
    /// </summary>
    public struct PointerDrag
    {
        /// <summary>
        /// 本次事件的输入上下文。
        /// </summary>
        public PointerInputContext Context;
    }

    /// <summary>
    /// 指针拖拽结束事件
    /// </summary>
    public struct PointerDragEnd
    {
        /// <summary>
        /// 本次事件的输入上下文。
        /// </summary>
        public PointerInputContext Context;
    }
}
