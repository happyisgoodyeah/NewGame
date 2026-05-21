using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// PuzzleView 的创建和基础状态维护逻辑。
    /// </summary>
    [EntitySystemOf(typeof(PuzzleView))]
    public static partial class PuzzleViewSystem
    {
        private const float RotationTweenDuration = 0.16f;
        private const float RotationTweenOvershoot = 0.55f;
        private const float BlockedRotationAngle = 30f;
        private const float BlockedRotationTweenDuration = 0.1f;

        /// <summary>
        /// 创建 Puzzle 场景对象，并绑定主图、多边形碰撞体和锚点结构
        /// </summary>
        [EntitySystem]
        private static void Awake(this PuzzleView self, GameObject prefab, Vector3 localPosition)
        {
            PuzzleSceneRoot puzzleSceneRoot = self.Scene().EnsureInitialized();
            GameObject instance = UnityEngine.Object.Instantiate(prefab);
            if (instance == null)
            {
                throw new UnityException("puzzle prefab instantiate failed");
            }

            instance.transform.SetParent(puzzleSceneRoot.SceneRoot, false);
            instance.name = $"{instance.name}_{self.GetParent<Puzzle>().Id}";
            instance.transform.localPosition = localPosition;

            self.GameObject = instance;
            self.Transform = instance.transform;
            self.SpriteRenderer = instance.GetComponent<SpriteRenderer>();
            self.PolygonCollider2D = instance.GetComponent<PolygonCollider2D>();
            if (self.PolygonCollider2D == null)
            {
                throw new UnityException("puzzle prefab must contain PolygonCollider2D");
            }

            self.BodyCollider2D = self.PolygonCollider2D;
            self.Rigidbody2D = instance.GetComponent<Rigidbody2D>();
            self.SlotAnchorTransforms = self.FindSlotAnchorTransforms();
            self.SlotAnchorTransform = self.FindOriginSlotAnchorTransform();
            self.DisableSlotAnchorColliders();
            self.MoveMode = PuzzleMoveMode.FreeFollow;
            self.RotationTween = null;
            self.BindEntityReference(instance);
            self.RefreshRotation();
            self.RestoreVisualPriority();
        }

        /// <summary>
        /// 释放 Puzzle 对应的场景对象。
        /// </summary>
        [EntitySystem]
        private static void Destroy(this PuzzleView self)
        {
            self.KillRotationTween();
            if (self.GameObject == null)
            {
                return;
            }

            UnityEngine.Object.Destroy(self.GameObject);
        }

        /// <summary>
        /// 将当前拼图的显示层级提升到当前根节点的最上层。
        /// </summary>
        /// <param name="self">当前 PuzzleView。</param>
        public static void BringToFront(this PuzzleView self)
        {
            if (self.Transform != null)
            {
                self.Transform.SetAsLastSibling();
            }

            if (self.SpriteRenderer != null)
            {
                self.SpriteRenderer.sortingOrder = PuzzleViewConst.DraggingSortingOrder;
            }
        }

        /// <summary>
        /// 将当前拼图恢复到默认显示层级。
        /// </summary>
        /// <param name="self">当前 PuzzleView。</param>
        public static void RestoreVisualPriority(this PuzzleView self)
        {
            if (self.SpriteRenderer != null)
            {
                self.SpriteRenderer.sortingOrder = PuzzleViewConst.DefaultSortingOrder;
            }
        }

        /// <summary>
        /// 根据当前 Puzzle 的离散旋转状态刷新表现层朝向，并保持原点格位置不变。
        /// </summary>
        /// <param name="self">当前 PuzzleView。</param>
        /// <param name="animated">是否使用 DOTween 播放旋转过渡。</param>
        public static void RefreshRotation(this PuzzleView self, bool animated = false)
        {
            Puzzle puzzle = self.GetParent<Puzzle>();
            if (self.Transform == null || puzzle == null)
            {
                return;
            }

            Vector3 originWorldPosition = self.GetOriginWorldPosition();
            float targetRotationZ = -90f * (int)puzzle.Rotation;
            if (!animated)
            {
                self.KillRotationTween();
                self.ApplyRotationKeepingOrigin(targetRotationZ, originWorldPosition);
                return;
            }

            self.PlayRotationTween(targetRotationZ, originWorldPosition);
        }

        /// <summary>
        /// 播放一次旋转失败的受阻表现，朝本次尝试方向旋转一小段后复原。
        /// </summary>
        /// <param name="self">当前 PuzzleView。</param>
        public static void PlayBlockedRotation(this PuzzleView self)
        {
            Puzzle puzzle = self.GetParent<Puzzle>();
            if (self.Transform == null || puzzle == null)
            {
                return;
            }

            Vector3 originWorldPosition = self.GetOriginWorldPosition();
            float baseRotationZ = -90f * (int)puzzle.Rotation;
            self.PlayBlockedRotationTween(baseRotationZ, originWorldPosition);
        }

        /// <summary>
        /// 直接按世界坐标设置 Puzzle 根节点的场景位置。
        /// </summary>
        /// <param name="self">当前 PuzzleView。</param>
        /// <param name="worldPosition">目标世界坐标。</param>
        public static void SetWorldPosition(this PuzzleView self, Vector3 worldPosition)
        {
            if (self.Transform == null)
            {
                return;
            }

            Transform parentTransform = self.Transform.parent;
            self.Transform.localPosition = parentTransform != null ? parentTransform.InverseTransformPoint(worldPosition) : worldPosition;
        }

        /// <summary>
        /// 直接按原点格锚点的世界坐标设置 Puzzle 的场景位置。
        /// </summary>
        /// <param name="self">当前 PuzzleView。</param>
        /// <param name="originWorldPosition">目标原点格世界坐标。</param>
        public static void SetOriginWorldPosition(this PuzzleView self, Vector3 originWorldPosition)
        {
            if (self.Transform == null)
            {
                return;
            }

            Vector3 currentOriginWorldPosition = self.GetOriginWorldPosition();
            Vector3 rootTargetWorldPosition = self.Transform.position + (originWorldPosition - currentOriginWorldPosition);
            self.SetWorldPosition(rootTargetWorldPosition);
        }

        /// <summary>
        /// 获取当前 Puzzle 指定形状格对应的场景锚点。
        /// </summary>
        /// <param name="self">当前 PuzzleView。</param>
        /// <param name="slot">要匹配的 Puzzle Slot。</param>
        /// <returns>当前形状格对应的场景锚点。</returns>
        public static Transform GetSlotAnchorTransform(this PuzzleView self, Slot slot)
        {
            if (slot == null)
            {
                return self.SlotAnchorTransform != null ? self.SlotAnchorTransform : self.Transform;
            }

            return self.FindBestSlotAnchorTransform(slot.X, slot.Y);
        }

        /// <summary>
        /// 获取当前 Puzzle 原点格锚点的世界坐标。
        /// </summary>
        /// <param name="self">当前 PuzzleView。</param>
        /// <returns>原点格锚点的世界坐标。</returns>
        public static Vector3 GetOriginWorldPosition(this PuzzleView self)
        {
            Puzzle puzzle = self.GetParent<Puzzle>();
            Slot originSlot = puzzle?.GetOriginSlot();
            Transform slotAnchorTransform = self.GetSlotAnchorTransform(originSlot);
            if (slotAnchorTransform != null)
            {
                return slotAnchorTransform.position;
            }

            return self.Transform != null ? self.Transform.position : Vector3.zero;
        }

        /// <summary>
        /// 收集 Puzzle prefab 中全部可用于匹配形状格的锚点。
        /// </summary>
        /// <param name="self">当前 PuzzleView。</param>
        /// <returns>可用的 Slot 锚点列表。</returns>
        public static List<Transform> FindSlotAnchorTransforms(this PuzzleView self)
        {
            List<Transform> results = new List<Transform>();
            if (self.Transform == null)
            {
                return results;
            }

            BoxCollider2D[] boxColliders = self.Transform.GetComponentsInChildren<BoxCollider2D>(true);
            foreach (BoxCollider2D boxCollider in boxColliders)
            {
                if (boxCollider.transform == self.Transform)
                {
                    continue;
                }

                if (!results.Contains(boxCollider.transform))
                {
                    results.Add(boxCollider.transform);
                }
            }

            if (results.Count == 0)
            {
                results.Add(self.Transform);
            }

            return results;
        }

        /// <summary>
        /// 关闭 Puzzle 子 Slot 的矩形锚点碰撞体，避免它们参与输入和吸附接触
        /// </summary>
        /// <param name="self">当前 PuzzleView</param>
        private static void DisableSlotAnchorColliders(this PuzzleView self)
        {
            if (self.Transform == null)
            {
                return;
            }

            BoxCollider2D[] boxColliders = self.Transform.GetComponentsInChildren<BoxCollider2D>(true);
            foreach (BoxCollider2D boxCollider in boxColliders)
            {
                if (boxCollider == null || boxCollider.transform == self.Transform)
                {
                    continue;
                }

                boxCollider.enabled = false;
            }
        }

        /// <summary>
        /// 查找当前 Puzzle 原点格优先使用的锚点。
        /// </summary>
        /// <param name="self">当前 PuzzleView。</param>
        /// <returns>原点格优先使用的锚点。</returns>
        public static Transform FindOriginSlotAnchorTransform(this PuzzleView self)
        {
            return self.FindBestSlotAnchorTransform(0, 0);
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

            if (entityRef == null)
            {
                throw new UnityException($"failed to add GameObjectEntityRef on puzzle object: {targetGameObject.name}");
            }

            entityRef.Entity = self;
        }

        /// <summary>
        /// 用 DOTween 播放一次离散旋转，并在动画过程中保持原点格锚点不漂移。
        /// </summary>
        /// <param name="self">当前 PuzzleView。</param>
        /// <param name="targetRotationZ">目标 Z 轴局部角度。</param>
        /// <param name="originWorldPosition">需要保持不变的原点格世界坐标。</param>
        private static void PlayRotationTween(this PuzzleView self, float targetRotationZ, Vector3 originWorldPosition)
        {
            self.KillRotationTween();
            float currentRotationZ = self.Transform.localEulerAngles.z;
            float endRotationZ = currentRotationZ + Mathf.DeltaAngle(currentRotationZ, targetRotationZ);
            self.RotationTween = DOTween.To(
                        () => currentRotationZ,
                        value =>
                        {
                            currentRotationZ = value;
                            self.ApplyRotationKeepingOrigin(value, originWorldPosition);
                        },
                        endRotationZ,
                        RotationTweenDuration)
                    .SetEase(Ease.OutBack, RotationTweenOvershoot)
                    .SetLink(self.GameObject)
                    .OnComplete(() =>
                    {
                        self.ApplyRotationKeepingOrigin(targetRotationZ, originWorldPosition);
                        self.RotationTween = null;
                    });
        }

        /// <summary>
        /// 用 DOTween 播放一次失败旋转的往返动画，并在过程中保持原点格锚点不漂移。
        /// </summary>
        /// <param name="self">当前 PuzzleView。</param>
        /// <param name="baseRotationZ">失败动画最终需要回到的基础角度。</param>
        /// <param name="originWorldPosition">需要保持不变的原点格世界坐标。</param>
        private static void PlayBlockedRotationTween(this PuzzleView self, float baseRotationZ, Vector3 originWorldPosition)
        {
            self.KillRotationTween();
            float currentRotationZ = self.Transform.localEulerAngles.z;
            float startRotationZ = currentRotationZ + Mathf.DeltaAngle(currentRotationZ, baseRotationZ);
            float blockedRotationZ = startRotationZ - BlockedRotationAngle;
            self.ApplyRotationKeepingOrigin(startRotationZ, originWorldPosition);
            self.RotationTween = DOTween.To(
                        () => startRotationZ,
                        value =>
                        {
                            startRotationZ = value;
                            self.ApplyRotationKeepingOrigin(value, originWorldPosition);
                        },
                        blockedRotationZ,
                        BlockedRotationTweenDuration)
                    .SetEase(Ease.OutQuad)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetLink(self.GameObject)
                    .OnComplete(() =>
                    {
                        self.ApplyRotationKeepingOrigin(baseRotationZ, originWorldPosition);
                        self.RotationTween = null;
                    });
        }

        /// <summary>
        /// 设置局部旋转角度，并把 Puzzle 原点格重新锚回指定世界坐标。
        /// </summary>
        /// <param name="self">当前 PuzzleView。</param>
        /// <param name="rotationZ">目标 Z 轴局部角度。</param>
        /// <param name="originWorldPosition">需要保持不变的原点格世界坐标。</param>
        private static void ApplyRotationKeepingOrigin(this PuzzleView self, float rotationZ, Vector3 originWorldPosition)
        {
            self.Transform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
            self.SetOriginWorldPosition(originWorldPosition);
        }

        /// <summary>
        /// 停止当前仍在播放的旋转 Tween。
        /// </summary>
        /// <param name="self">当前 PuzzleView。</param>
        private static void KillRotationTween(this PuzzleView self)
        {
            if (self.RotationTween == null)
            {
                return;
            }

            if (self.RotationTween.IsActive())
            {
                self.RotationTween.Kill(false);
            }

            self.RotationTween = null;
        }

        /// <summary>
        /// 按 Puzzle Slot 的局部坐标匹配最接近的 prefab 锚点。
        /// </summary>
        /// <param name="self">当前 PuzzleView。</param>
        /// <param name="slotX">目标形状格的局部 X 坐标。</param>
        /// <param name="slotY">目标形状格的局部 Y 坐标。</param>
        /// <returns>最匹配的 prefab 锚点。</returns>
        private static Transform FindBestSlotAnchorTransform(this PuzzleView self, int slotX, int slotY)
        {
            if (self.SlotAnchorTransforms == null || self.SlotAnchorTransforms.Count == 0)
            {
                return self.Transform;
            }

            Vector3 expectedLocalPosition = new Vector3(slotX * PuzzleViewConst.GridCellSize, -slotY * PuzzleViewConst.GridCellSize, 0f);
            Transform bestTransform = self.SlotAnchorTransforms[0];
            float bestDistance = float.MaxValue;
            foreach (Transform slotAnchorTransform in self.SlotAnchorTransforms)
            {
                if (slotAnchorTransform == null)
                {
                    continue;
                }

                float distance = Vector3.Distance(slotAnchorTransform.localPosition, expectedLocalPosition);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestTransform = slotAnchorTransform;
                }
            }

            return bestTransform != null ? bestTransform : self.Transform;
        }
    }
}
