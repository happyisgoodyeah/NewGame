namespace ET.Client
{
    /// <summary>
    /// Slot 数据实体创建完成后，为其创建对应的 SlotView。
    /// </summary>
    [Event(SceneType.Current)]
    public class AfterCreateSlot_CreateSlotView : AEvent<Scene, AfterCreateSlot>
    {
        /// <summary>
        /// 等待 Slot 所属父实体的 View 可用后创建 SlotView。
        /// </summary>
        protected override async ETTask Run(Scene scene, AfterCreateSlot args)
        {
            Slot slot = args.Slot;
            await scene.WaitUntil(() => scene.IsDisposed || slot == null || slot.IsDisposed || IsParentViewReady(slot));
            if (scene.IsDisposed || slot == null || slot.IsDisposed || slot.GetComponent<SlotView>() != null)
            {
                return;
            }

            if (!IsParentViewReady(slot))
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

            if (scene.IsDisposed || slot.IsDisposed || slot.GetComponent<SlotView>() != null || !IsParentViewReady(slot))
            {
                return;
            }

            slot.AddComponent<SlotView, UnityEngine.GameObject>(prefab);
        }

        /// <summary>
        /// 判断当前 Slot 所属父实体的表现组件是否已经创建。
        /// </summary>
        /// <param name="slot">待检查的 Slot 数据实体。</param>
        /// <returns>父级表现组件是否可用。</returns>
        private static bool IsParentViewReady(Slot slot)
        {
            if (slot == null || slot.IsDisposed)
            {
                return false;
            }

            if (slot.Parent is Grid grid)
            {
                return grid.GetComponent<GridView>() != null;
            }

            if (slot.Parent is Puzzle puzzle)
            {
                return puzzle.GetComponent<PuzzleView>() != null;
            }

            return false;
        }
    }
}
