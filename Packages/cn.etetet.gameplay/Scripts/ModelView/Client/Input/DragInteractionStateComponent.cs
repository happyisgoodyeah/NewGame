namespace ET.Client
{
    /// <summary>
    /// 场景级拖拽交互状态组件，负责记录当前拖拽手势锁定的业务目标
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class DragInteractionStateComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 当前拖拽手势锁定的业务目标
        /// </summary>
        public EntityRef<Entity> DraggingTarget { get; set; }
    }
}
