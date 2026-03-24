using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// SlotView 的创建和绑定逻辑。
    /// </summary>
    [EntitySystemOf(typeof(SlotView))]
    public static partial class SlotViewSystem
    {
        /// <summary>
        /// 根据 Slot 的父对象类型，创建或绑定对应的场景节点。
        /// </summary>
        [EntitySystem]
        private static void Awake(this SlotView self)
        {
            Slot slot = self.GetParent<Slot>();
            if (slot.IsGridSlot())
            {
                self.BindGridSlotView(slot);
                return;
            }

            if (slot.IsPuzzleSlot())
            {
                self.BindPuzzleSlotView(slot);
            }
        }

        /// <summary>
        /// 根据绑定方式决定是否销毁当前场景对象。
        /// </summary>
        [EntitySystem]
        private static void Destroy(this SlotView self)
        {
            if (self.OwnsGameObject && self.GameObject != null)
            {
                UnityEngine.Object.Destroy(self.GameObject);
            }
        }

        /// <summary>
        /// 为 Grid Slot 实例化单独的 Slot1001 prefab。
        /// </summary>
        private static void BindGridSlotView(this SlotView self, Slot slot)
        {
            Grid grid = slot.GetParent<Grid>();
            GridView gridView = grid.GetComponent<GridView>();
            if (gridView == null)
            {
                throw new UnityException("grid view is required before creating grid slot view");
            }

            GameObject prefab = Resources.Load<GameObject>(PuzzleViewConst.GridSlotPrefabPath);
            if (prefab == null)
            {
                throw new UnityException($"grid slot prefab not found: {PuzzleViewConst.GridSlotPrefabPath}");
            }

            GameObject instance = UnityEngine.Object.Instantiate(prefab, gridView.SlotRoot, false);
            instance.name = $"GridSlot_{slot.X}_{slot.Y}";
            instance.transform.localPosition = gridView.GetLocalPosition(slot);

            self.GameObject = instance;
            self.Transform = instance.transform;
            self.Collider2D = instance.GetComponent<Collider2D>();
            if (self.Collider2D is BoxCollider2D boxCollider2D)
            {
                boxCollider2D.usedByComposite = true;
            }

            self.OwnsGameObject = true;
            self.BindEntityReference();
            self.BindCollisionMarker(CollisionMarkerGroup.GridSlot, true);
        }

        /// <summary>
        /// 为 Puzzle Slot 绑定 Puzzle prefab 内已经存在的结构节点。
        /// </summary>
        private static void BindPuzzleSlotView(this SlotView self, Slot slot)
        {
            Puzzle puzzle = slot.GetParent<Puzzle>();
            PuzzleView puzzleView = puzzle.GetComponent<PuzzleView>();
            if (puzzleView == null)
            {
                throw new UnityException("puzzle view is required before binding puzzle slot view");
            }

            Transform slotAnchor = puzzleView.SlotAnchorTransform != null ? puzzleView.SlotAnchorTransform : puzzleView.Transform;
            self.GameObject = slotAnchor.gameObject;
            self.Transform = slotAnchor;
            self.Collider2D = slotAnchor.GetComponent<Collider2D>();
            self.OwnsGameObject = false;
            self.BindEntityReference();
            self.BindCollisionMarker(CollisionMarkerGroup.PuzzleSlot, true);
        }

        /// <summary>
        /// 为当前 SlotView 挂载 Unity 到 ET 的实体桥接组件。
        /// </summary>
        /// <param name="self">当前 SlotView。</param>
        private static void BindEntityReference(this SlotView self)
        {
            if (self.GameObject == null)
            {
                return;
            }

            GameObjectEntityRef entityRef = self.GameObject.GetComponent<GameObjectEntityRef>();
            if (entityRef == null)
            {
                entityRef = self.GameObject.AddComponent<GameObjectEntityRef>();
            }

            entityRef.Entity = self;
        }

        /// <summary>
        /// 为当前 SlotView 绑定对象挂载通用碰撞标记。
        /// </summary>
        /// <param name="self">当前 SlotView。</param>
        /// <param name="group">碰撞分组。</param>
        /// <param name="primary">是否为主碰撞体。</param>
        private static void BindCollisionMarker(this SlotView self, string group, bool primary)
        {
            if (self.GameObject == null)
            {
                return;
            }

            CollisionMarker collisionMarker = self.GameObject.GetComponent<CollisionMarker>();
            if (collisionMarker == null)
            {
                collisionMarker = self.GameObject.AddComponent<CollisionMarker>();
            }

            collisionMarker.Group = group;
            collisionMarker.Primary = primary;
            self.CollisionMarker = collisionMarker;
        }
    }
}
