namespace ET.Client
{
    /// <summary>
    /// Grid 数据实体创建完成后，为其创建对应的 GridView。
    /// </summary>
    [Event(SceneType.Current)]
    public class AfterCreateGrid_CreateGridView : AEvent<Scene, AfterCreateGrid>
    {
        /// <summary>
        /// 等待 Unity 场景可用后创建 GridView，避免数据创建早于场景加载导致表现层缺失。
        /// </summary>
        protected override async ETTask Run(Scene scene, AfterCreateGrid args)
        {
            Grid grid = args.Grid;
            await scene.WaitUntil(() => scene.IsDisposed || grid == null || grid.IsDisposed || scene.IsPuzzleViewReady());
            if (scene.IsDisposed || grid == null || grid.IsDisposed || grid.GetComponent<GridView>() != null)
            {
                return;
            }

            GridConfig gridConfig = PuzzleCoreHelper.GetGridConfig(grid.GridConfigId);
            string assetLocation = ResourceLocationHelper.ToAssetLocation(gridConfig.PrefabPath);
            UnityEngine.GameObject prefab = await scene.GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<UnityEngine.GameObject>(assetLocation);
            if (scene.IsDisposed || grid.IsDisposed || grid.GetComponent<GridView>() != null)
            {
                return;
            }

            grid.AddComponent<GridView, UnityEngine.GameObject, UnityEngine.Vector3, float>(prefab, UnityEngine.Vector3.zero, PuzzleViewConst.GridCellSize);
        }
    }
}
