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
        private static void Awake(this SlotView self, GameObject prefab)
        {
            Slot slot = self.GetParent<Slot>();
            if (slot.IsGridSlot())
            {
                self.BindGridSlotView(slot, prefab);
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
            if (!self.OwnsGameObject || self.GameObject == null)
            {
                return;
            }

            UnityEngine.Object.Destroy(self.GameObject);
        }

        /// <summary>
        /// 为 Grid Slot 实例化配置指定的 Slot prefab，并绑定独立 BoxCollider2D
        /// </summary>
        private static void BindGridSlotView(this SlotView self, Slot slot, GameObject prefab)
        {
            Grid grid = slot.GetParent<Grid>();
            GridView gridView = grid.GetComponent<GridView>();
            if (gridView == null)
            {
                throw new UnityException("grid view is required before creating grid slot view");
            }

            GameObject instance = UnityEngine.Object.Instantiate(prefab, gridView.SlotRoot, false);
            if (instance == null)
            {
                throw new UnityException("grid slot prefab instantiate failed");
            }

            instance.name = $"GridSlot_{slot.X}_{slot.Y}";
            instance.transform.localPosition = gridView.GetLocalPosition(slot);

            self.GameObject = instance;
            self.Transform = instance.transform;
            BoxCollider2D boxCollider2D = instance.GetComponent<BoxCollider2D>();
            if (boxCollider2D == null)
            {
                throw new UnityException("grid slot prefab must contain BoxCollider2D");
            }

            boxCollider2D.usedByComposite = false;
            self.Collider2D = boxCollider2D;
            self.OwnsGameObject = true;
            self.BindEntityReference();
        }

        /// <summary>
        /// 为 Puzzle Slot 绑定 Puzzle prefab 内已存在的对应结构节点。
        /// </summary>
        private static void BindPuzzleSlotView(this SlotView self, Slot slot)
        {
            Puzzle puzzle = slot.GetParent<Puzzle>();
            PuzzleView puzzleView = puzzle.GetComponent<PuzzleView>();
            if (puzzleView == null)
            {
                throw new UnityException("puzzle view is required before binding puzzle slot view");
            }

            Transform slotAnchor = puzzleView.GetSlotAnchorTransform(slot);
            self.GameObject = slotAnchor != null ? slotAnchor.gameObject : puzzleView.GameObject;
            self.Transform = slotAnchor != null ? slotAnchor : puzzleView.Transform;
            self.Collider2D = self.Transform != null ? self.Transform.GetComponent<Collider2D>() : null;
            self.OwnsGameObject = false;
            self.BindEntityReference();
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

            if (entityRef == null)
            {
                throw new UnityException($"failed to add GameObjectEntityRef on slot object: {self.GameObject.name}");
            }

            entityRef.Entity = self;
        }
    }
}
