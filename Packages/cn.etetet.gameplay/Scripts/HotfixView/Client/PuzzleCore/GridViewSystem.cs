using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// GridView 的创建和坐标换算逻辑
    /// </summary>
    [EntitySystemOf(typeof(GridView))]
    public static partial class GridViewSystem
    {
        /// <summary>
        /// 创建 Grid 的场景根节点和 Grid Slot 的挂载容器
        /// </summary>
        [EntitySystem]
        private static void Awake(this GridView self, GameObject prefab, Vector3 localPosition, float cellSize)
        {
            PuzzleSceneRoot puzzleSceneRoot = self.Scene().InitializePuzzleSceneRoot();
            GameObject instance = UnityEngine.Object.Instantiate(prefab);
            if (instance == null)
            {
                throw new UnityException("grid prefab instantiate failed");
            }

            instance.transform.SetParent(puzzleSceneRoot.GridRoot, false);
            instance.name = $"{instance.name}_{self.GetParent<Grid>().Id}";
            instance.transform.localPosition = localPosition;

            Transform puzzleRoot = instance.transform.Find("PuzzleTransform");
            Transform slotRoot = instance.transform.Find("SlotTransform");
            if (puzzleRoot == null || slotRoot == null)
            {
                throw new UnityException("grid prefab must contain PuzzleTransform and SlotTransform");
            }

            CompositeCollider2D compositeCollider2D = slotRoot.GetComponent<CompositeCollider2D>();
            if (compositeCollider2D == null)
            {
                throw new UnityException("grid prefab SlotTransform must contain CompositeCollider2D");
            }

            self.GameObject = instance;
            self.Transform = instance.transform;
            self.PuzzleRoot = puzzleRoot;
            self.SlotRoot = slotRoot;
            self.CompositeCollider2D = compositeCollider2D;
            self.CellSize = cellSize;
            self.BindEntityReference(instance);
        }

        /// <summary>
        /// 释放 Grid 的场景根节点
        /// </summary>
        [EntitySystem]
        private static void Destroy(this GridView self)
        {
            if (self.GameObject == null)
            {
                return;
            }

            UnityEngine.Object.Destroy(self.GameObject);
        }

        /// <summary>
        /// 计算某个 Grid Slot 在 GridRoot 下的局部位置
        /// </summary>
        public static Vector3 GetLocalPosition(this GridView self, Slot slot)
        {
            return self.GetLocalPosition(slot.X, slot.Y);
        }

        /// <summary>
        /// 计算指定 Grid 坐标在 GridRoot 下的局部位置，允许坐标落在 Grid 外一圈用于吸附预览
        /// </summary>
        /// <param name="self">当前 Grid 的表现组件</param>
        /// <param name="x">目标 Grid X 坐标</param>
        /// <param name="y">目标 Grid Y 坐标</param>
        /// <returns>目标坐标对应的局部位置</returns>
        public static Vector3 GetLocalPosition(this GridView self, int x, int y)
        {
            Grid grid = self.GetParent<Grid>();
            float localX = (x - (grid.Width - 1) * 0.5f) * self.CellSize;
            float localY = ((grid.Height - 1) * 0.5f - y) * self.CellSize;
            return new Vector3(localX, localY, 0f);
        }

        /// <summary>
        /// 计算某个 Grid Slot 中心点在世界坐标系中的位置
        /// </summary>
        /// <param name="self">当前 Grid 的表现组件</param>
        /// <param name="slot">要计算世界坐标的 Grid Slot</param>
        /// <returns>目标 Grid Slot 中心点的世界坐标</returns>
        public static Vector3 GetSlotWorldPosition(this GridView self, Slot slot)
        {
            return self.GetGridCoordinateWorldPosition(slot.X, slot.Y);
        }

        /// <summary>
        /// 计算指定 Grid 坐标中心点在世界坐标系中的位置，允许坐标位于 Grid 外一圈
        /// 将 Puzzle 的 OriginSlot 对齐到 Grid 的 anchorX / anchorY 这个格子中心
        /// </summary>
        /// <param name="self">当前 Grid 的表现组件</param>
        /// <param name="x">目标 Grid X 坐标</param>
        /// <param name="y">目标 Grid Y 坐标</param>
        /// <returns>目标坐标中心点的世界坐标</returns>
        public static Vector3 GetGridCoordinateWorldPosition(this GridView self, int x, int y)
        {
            Vector3 localPosition = self.GetLocalPosition(x, y);
            return self.SlotRoot != null ? self.SlotRoot.TransformPoint(localPosition) : self.Transform.TransformPoint(localPosition);
        }

        /// <summary>
        /// 计算某个 Grid Slot 对应的外围吸附停靠位世界坐标
        /// </summary>
        /// <param name="self">当前 Grid 的表现组件</param>
        /// <param name="slot">当前候选原点格</param>
        /// <param name="region">外围吸附区域</param>
        /// <returns>当前边/角停靠位的世界坐标</returns>
        public static Vector3 GetPreviewSnapWorldPosition(this GridView self, Slot slot, GridContactRegion region)
        {
            Vector3 slotWorldPosition = self.GetSlotWorldPosition(slot);
            Vector3 offset = region switch
            {
                GridContactRegion.Left => new Vector3(-self.CellSize, 0f, 0f),
                GridContactRegion.Right => new Vector3(self.CellSize, 0f, 0f),
                GridContactRegion.Top => new Vector3(0f, self.CellSize, 0f),
                GridContactRegion.Bottom => new Vector3(0f, -self.CellSize, 0f),
                GridContactRegion.LeftTop => new Vector3(-self.CellSize, self.CellSize, 0f),
                GridContactRegion.RightTop => new Vector3(self.CellSize, self.CellSize, 0f),
                GridContactRegion.LeftBottom => new Vector3(-self.CellSize, -self.CellSize, 0f),
                GridContactRegion.RightBottom => new Vector3(self.CellSize, -self.CellSize, 0f),
                _ => Vector3.zero,
            };
            return slotWorldPosition + offset;
        }

        /// <summary>
        /// 获取当前 GridSnap 阶段用于计算鼠标步进的参考世界坐标
        /// </summary>
        /// <param name="self">当前 Grid 的表现组件</param>
        /// <param name="slot">当前候选原点格</param>
        /// <param name="region">当前外围吸附区域</param>
        /// <returns>用于步进判定的参考世界坐标</returns>
        public static Vector3 GetSnapReferenceWorldPosition(this GridView self, Slot slot, GridContactRegion region)
        {
            return region == GridContactRegion.Center ? self.GetSlotWorldPosition(slot) : self.GetPreviewSnapWorldPosition(slot, region);
        }

        /// <summary>
        /// 为 Grid 根节点挂载 Unity 到 ET 的实体桥接组件
        /// </summary>
        /// <param name="self">当前 GridView</param>
        /// <param name="targetGameObject">要绑定的对象</param>
        private static void BindEntityReference(this GridView self, GameObject targetGameObject)
        {
            if (targetGameObject == null)
            {
                return;
            }

            GameObjectEntityRef entityRef = targetGameObject.GetComponent<GameObjectEntityRef>();
            if (entityRef == null)
            {
                entityRef = targetGameObject.AddComponent<GameObjectEntityRef>();
            }

            if (entityRef == null)
            {
                throw new UnityException($"failed to add GameObjectEntityRef on grid object: {targetGameObject.name}");
            }

            entityRef.Entity = self;
        }
    }
}
