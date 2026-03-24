using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// GridView 的创建和坐标换算逻辑。
    /// </summary>
    [EntitySystemOf(typeof(GridView))]
    public static partial class GridViewSystem
    {
        /// <summary>
        /// 创建 Grid 的场景根节点和 Grid Slot 的挂载容器。
        /// </summary>
        [EntitySystem]
        private static void Awake(this GridView self, Vector3 localPosition, float cellSize)
        {
            PuzzleSceneRoot puzzleSceneRoot = self.Scene().EnsureInitialized();
            GameObject prefab = Resources.Load<GameObject>(PuzzleViewConst.GridPrefabPath);
            if (prefab == null)
            {
                throw new UnityException($"grid prefab not found: {PuzzleViewConst.GridPrefabPath}");
            }

            GameObject root = UnityEngine.Object.Instantiate(prefab, puzzleSceneRoot.GridRoot, false);
            root.name = $"{prefab.name}_{self.GetParent<Grid>().Id}";
            root.transform.localPosition = localPosition;

            Transform puzzleRoot = root.transform.Find("PuzzleTransform");
            Transform slotRoot = root.transform.Find("SlotTransform");
            if (puzzleRoot == null || slotRoot == null)
            {
                throw new UnityException("grid prefab must contain PuzzleTransform and SlotTransform");
            }

            CompositeCollider2D compositeCollider2D = slotRoot.GetComponent<CompositeCollider2D>();
            if (compositeCollider2D == null)
            {
                throw new UnityException("grid prefab SlotTransform must contain CompositeCollider2D");
            }

            self.GameObject = root;
            self.Transform = root.transform;
            self.PuzzleRoot = puzzleRoot;
            self.SlotRoot = slotRoot;
            self.CompositeCollider2D = compositeCollider2D;
            self.CellSize = cellSize;
            self.BindEntityReference(root);
            self.BindGridAreaMarker(slotRoot.gameObject);
        }

        /// <summary>
        /// 释放 Grid 的场景根节点。
        /// </summary>
        [EntitySystem]
        private static void Destroy(this GridView self)
        {
            if (self.GameObject != null)
            {
                UnityEngine.Object.Destroy(self.GameObject);
            }
        }

        /// <summary>
        /// 计算某个 Grid Slot 在 GridRoot 下的局部位置。
        /// </summary>
        public static Vector3 GetLocalPosition(this GridView self, Slot slot)
        {
            Grid grid = self.GetParent<Grid>();
            float x = (slot.X - (grid.Width - 1) * 0.5f) * self.CellSize;
            float y = ((grid.Height - 1) * 0.5f - slot.Y) * self.CellSize;
            return new Vector3(x, y, 0f);
        }

        /// <summary>
        /// 计算某个 Grid Slot 中心点在世界坐标系中的位置。
        /// </summary>
        /// <param name="self">当前 Grid 的表现组件。</param>
        /// <param name="slot">要计算世界坐标的 Grid Slot。</param>
        /// <returns>目标 Grid Slot 中心点的世界坐标。</returns>
        public static Vector3 GetSlotWorldPosition(this GridView self, Slot slot)
        {
            Vector3 localPosition = self.GetLocalPosition(slot);
            return self.SlotRoot != null ? self.SlotRoot.TransformPoint(localPosition) : self.Transform.TransformPoint(localPosition);
        }

        /// <summary>
        /// 为 Grid 根节点挂载 Unity 到 ET 的实体桥接组件。
        /// </summary>
        /// <param name="self">当前 GridView。</param>
        /// <param name="targetGameObject">要绑定的对象。</param>
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

            entityRef.Entity = self;
        }

        /// <summary>
        /// 为 Grid 的整体接触区域挂载通用碰撞标记。
        /// </summary>
        /// <param name="self">当前 GridView。</param>
        /// <param name="targetGameObject">Grid 区域对象。</param>
        private static void BindGridAreaMarker(this GridView self, GameObject targetGameObject)
        {
            if (targetGameObject == null)
            {
                return;
            }

            CollisionMarker collisionMarker = targetGameObject.GetComponent<CollisionMarker>();
            if (collisionMarker == null)
            {
                collisionMarker = targetGameObject.AddComponent<CollisionMarker>();
            }

            collisionMarker.Group = CollisionMarkerGroup.GridArea;
            collisionMarker.Primary = true;
        }
    }
}
