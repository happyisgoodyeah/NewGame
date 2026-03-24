using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// Grid 数据创建完成后，根据数据生成 GridView。
    /// </summary>
    [Event(SceneType.Current)]
    public class AfterCreateGrid_CreateGridView : AEvent<Scene, AfterCreateGrid>
    {
        /// <summary>
        /// 根据 Grid 数据创建位于场景中心的 GridView。
        /// </summary>
        protected override async ETTask Run(Scene scene, AfterCreateGrid args)
        {
            if (!PuzzleViewEventHelper.IsSceneReady(scene))
            {
                await ETTask.CompletedTask;
                return;
            }

            Grid grid = args.Grid;
            if (grid.GetComponent<GridView>() == null)
            {
                Vector3 worldPosition = PuzzleViewLayoutHelper.GetGridWorldPosition();
                grid.AddComponent<GridView, Vector3, float>(worldPosition, PuzzleViewConst.GridCellSize);
            }

            await ETTask.CompletedTask;
        }
    }
}
