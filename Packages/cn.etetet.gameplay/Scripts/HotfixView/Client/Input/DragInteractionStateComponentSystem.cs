namespace ET.Client
{
    /// <summary>
    /// DragInteractionStateComponent 的状态维护与快捷访问逻辑
    /// </summary>
    [EntitySystemOf(typeof(DragInteractionStateComponent))]
    public static partial class DragInteractionStateComponentSystem
    {
        /// <summary>
        /// 初始化当前场景的拖拽交互状态
        /// </summary>
        [EntitySystem]
        private static void Awake(this DragInteractionStateComponent self)
        {
            self.DraggingTarget = default;
        }

        /// <summary>
        /// 清理当前场景的拖拽交互状态
        /// </summary>
        [EntitySystem]
        private static void Destroy(this DragInteractionStateComponent self)
        {
            self.DraggingTarget = default;
        }

        /// <summary>
        /// 获取或挂载当前场景的拖拽交互状态组件
        /// </summary>
        /// <param name="self">当前场景</param>
        /// <returns>当前场景对应的拖拽交互状态组件</returns>
        public static DragInteractionStateComponent GetOrAddDragInteractionState(this Scene self)
        {
            DragInteractionStateComponent interactionStateComponent = self.GetComponent<DragInteractionStateComponent>();
            if (interactionStateComponent == null)
            {
                interactionStateComponent = self.AddComponent<DragInteractionStateComponent>();
            }

            return interactionStateComponent;
        }

        /// <summary>
        /// 获取当前拖拽手势锁定的业务目标
        /// </summary>
        /// <param name="self">当前场景的拖拽交互状态组件</param>
        /// <returns>当前拖拽目标；若不存在则返回空</returns>
        public static Entity GetDraggingTarget(this DragInteractionStateComponent self)
        {
            return self.DraggingTarget;
        }

        /// <summary>
        /// 记录当前拖拽手势锁定的业务目标
        /// </summary>
        /// <param name="self">当前场景的拖拽交互状态组件</param>
        /// <param name="target">要记录为正在拖拽的业务目标</param>
        public static void SetDraggingTarget(this DragInteractionStateComponent self, Entity target)
        {
            self.DraggingTarget = target;
        }

        /// <summary>
        /// 清空当前拖拽手势锁定的业务目标
        /// </summary>
        /// <param name="self">当前场景的拖拽交互状态组件</param>
        public static void ClearDraggingTarget(this DragInteractionStateComponent self)
        {
            self.DraggingTarget = default;
        }
    }
}
