namespace ET.Client
{
    /// <summary>
    /// Slot 数据创建完成后，根据数据生成或绑定 SlotView。
    /// </summary>
    [Event(SceneType.Current)]
    public class AfterCreateSlot_CreateSlotView : AEvent<Scene, AfterCreateSlot>
    {
        /// <summary>
        /// 根据 Slot 父对象类型创建 Grid SlotView，或绑定 Puzzle prefab 中已有的 Slot 节点。
        /// </summary>
        protected override async ETTask Run(Scene scene, AfterCreateSlot args)
        {
            if (!PuzzleViewEventHelper.IsSceneReady(scene))
            {
                await ETTask.CompletedTask;
                return;
            }

            Slot slot = args.Slot;
            if (slot.GetComponent<SlotView>() == null)
            {
                slot.AddComponent<SlotView>();
            }

            await ETTask.CompletedTask;
        }
    }
}
