using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// PuzzleView 的创建和基础状态维护逻辑。
    /// </summary>
    [EntitySystemOf(typeof(PuzzleView))]
    public static partial class PuzzleViewSystem
    {
        /// <summary>
        /// 创建 Puzzle 场景对象，并绑定主图和碰撞组件。
        /// </summary>
        [EntitySystem]
        private static void Awake(this PuzzleView self, string prefabPath, Vector3 localPosition)
        {
            PuzzleSceneRoot puzzleSceneRoot = self.Scene().EnsureInitialized();
            GameObject prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab == null)
            {
                throw new UnityException($"puzzle prefab not found: {prefabPath}");
            }

            GameObject instance = UnityEngine.Object.Instantiate(prefab, puzzleSceneRoot.SceneRoot, false);
            instance.name = $"{prefab.name}_{self.GetParent<Puzzle>().Id}";
            instance.transform.localPosition = localPosition;

            self.GameObject = instance;
            self.Transform = instance.transform;
            self.SpriteRenderer = instance.GetComponent<SpriteRenderer>();
            self.PolygonCollider2D = instance.GetComponent<PolygonCollider2D>();
            self.BodyCollider2D = self.PolygonCollider2D;
            self.Rigidbody2D = instance.GetComponent<Rigidbody2D>();
            self.SlotAnchorTransform = self.FindSlotAnchorTransform();
            self.MoveMode = PuzzleMoveMode.FreeFollow;
            self.BindEntityReference(instance);
            self.BindCollisionMarker(instance, CollisionMarkerGroup.PuzzleBody, true);
            self.BindSlotAnchorMarker();
        }

        /// <summary>
        /// 释放 Puzzle 对应的场景对象。
        /// </summary>
        [EntitySystem]
        private static void Destroy(this PuzzleView self)
        {
            if (self.GameObject != null)
            {
                UnityEngine.Object.Destroy(self.GameObject);
            }
        }

        /// <summary>
        /// 设置 Puzzle 在父节点局部坐标中的位置。
        /// </summary>
        public static void SetWorldPosition(this PuzzleView self, Vector3 localPosition)
        {
            if (self.Transform != null)
            {
                self.Transform.localPosition = localPosition;
            }
        }

        /// <summary>
        /// 查找当前默认 1x1 Puzzle 对应的 Slot 锚点。
        /// </summary>
        public static Transform FindSlotAnchorTransform(this PuzzleView self)
        {
            BoxCollider2D[] boxColliders = self.Transform.GetComponentsInChildren<BoxCollider2D>(true);
            foreach (BoxCollider2D boxCollider in boxColliders)
            {
                if (boxCollider.transform != self.Transform)
                {
                    return boxCollider.transform;
                }
            }

            return self.Transform;
        }

        /// <summary>
        /// 为 Puzzle 的场景根节点挂载 Unity 到 ET 的实体桥接组件。
        /// </summary>
        /// <param name="self">当前 PuzzleView。</param>
        /// <param name="targetGameObject">需要挂载桥接的目标对象。</param>
        private static void BindEntityReference(this PuzzleView self, GameObject targetGameObject)
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
        /// 为指定对象挂载通用碰撞标记。
        /// </summary>
        /// <param name="self">当前 PuzzleView。</param>
        /// <param name="targetGameObject">要挂载标记的对象。</param>
        /// <param name="group">碰撞分组。</param>
        /// <param name="primary">是否为主碰撞体。</param>
        private static void BindCollisionMarker(this PuzzleView self, GameObject targetGameObject, string group, bool primary)
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

            collisionMarker.Group = group;
            collisionMarker.Primary = primary;
        }

        /// <summary>
        /// 为 Puzzle 内部的 Slot 锚点补齐碰撞标记。
        /// </summary>
        /// <param name="self">当前 PuzzleView。</param>
        private static void BindSlotAnchorMarker(this PuzzleView self)
        {
            if (self.SlotAnchorTransform == null)
            {
                return;
            }

            self.BindCollisionMarker(self.SlotAnchorTransform.gameObject, CollisionMarkerGroup.PuzzleSlot, true);
        }
    }
}
