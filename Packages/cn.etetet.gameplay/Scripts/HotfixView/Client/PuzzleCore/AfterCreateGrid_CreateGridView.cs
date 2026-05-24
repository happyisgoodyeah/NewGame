namespace ET.Client
{
    /// <summary>
    /// Grid 数据实体创建完成后，为其创建对应的 GridView。
    /// </summary>
    [Event(SceneType.Current)]
    public class AfterCreateGrid_CreateGridView : AEvent<Scene, AfterCreateGrid>
    {
        /// <summary>
        /// 在 Grid 数据创建完成后立即加载 GridView
        /// </summary>
        protected override async ETTask Run(Scene scene, AfterCreateGrid args)
        {
            Grid grid = args.Grid;
            if (scene.IsDisposed || grid == null || grid.IsDisposed || grid.GetComponent<GridView>() != null)
            {
                return;
            }

            if (!scene.IsPuzzleViewReady())
            {
                Log.Error($"puzzle view scene is not ready: scene={scene.Name}");
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
