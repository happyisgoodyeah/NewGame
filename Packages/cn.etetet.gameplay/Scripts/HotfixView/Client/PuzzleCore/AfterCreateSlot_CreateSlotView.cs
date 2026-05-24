namespace ET.Client
{
    /// <summary>
    /// Slot 数据实体创建完成后，为其创建对应的 SlotView。
    /// </summary>
    [Event(SceneType.Current)]
    public class AfterCreateSlot_CreateSlotView : AEvent<Scene, AfterCreateSlot>
    {
        /// <summary>
        /// 在 Slot 数据创建完成后立即加载或绑定 SlotView
        /// </summary>
        protected override async ETTask Run(Scene scene, AfterCreateSlot args)
        {
            Slot slot = args.Slot;
            if (scene.IsDisposed || slot == null || slot.IsDisposed || slot.GetComponent<SlotView>() != null)
            {
                return;
            }

            UnityEngine.GameObject prefab = null;
            if (slot.IsGridSlot())
            {
                SlotConfig slotConfig = PuzzleCoreHelper.GetSlotConfig(slot.SlotConfigId);
                string assetLocation = ResourceLocationHelper.ToAssetLocation(slotConfig.PrefabPath);
                prefab = await scene.GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<UnityEngine.GameObject>(assetLocation);
            }

            if (scene.IsDisposed || slot.IsDisposed || slot.GetComponent<SlotView>() != null)
            {
                return;
            }

            slot.AddComponent<SlotView, UnityEngine.GameObject>(prefab);
        }
    }
}
