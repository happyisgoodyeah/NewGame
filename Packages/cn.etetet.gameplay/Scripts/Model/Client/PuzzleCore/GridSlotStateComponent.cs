namespace ET.Client
{
    /// <summary>
    /// Grid Slot 的运行时状态标记组件，用于承载 Grid 专属判定逻辑入口。
    /// </summary>
    [ComponentOf(typeof(Slot))]
    public class GridSlotStateComponent : Entity, IAwake, IDestroy
    {
    }
}
