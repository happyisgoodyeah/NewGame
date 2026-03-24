using System;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 通用输入组件系统，负责采集原始输入并发布输入事件。
    /// </summary>
    [EntitySystemOf(typeof(InputComponent))]
    public static partial class InputComponentSystem
    {
        private const float ClickDurationThreshold = 0.35f;
        private const float ClickScreenDistanceThreshold = 12f;

        /// <summary>
        /// 初始化通用输入组件的默认状态。
        /// </summary>
        [EntitySystem]
        private static void Awake(this InputComponent self)
        {
            self.MainCamera = null;
            self.PhysicsLayerMask = Physics2D.AllLayers;
            self.IsPointerPressed = false;
            self.PressScreenPosition = Vector2.zero;
            self.PressWorldPosition = Vector3.zero;
            self.PressStartTime = 0f;
        }

        /// <summary>
        /// 清理通用输入组件缓存的运行时状态。
        /// </summary>
        [EntitySystem]
        private static void Destroy(this InputComponent self)
        {
            self.MainCamera = null;
            self.IsPointerPressed = false;
            self.PressScreenPosition = Vector2.zero;
            self.PressWorldPosition = Vector3.zero;
            self.PressStartTime = 0f;
        }

        /// <summary>
        /// 每帧采集鼠标主按钮输入并发布对应的通用输入事件。
        /// </summary>
        [EntitySystem]
        private static void Update(this InputComponent self)
        {
            if (!self.TryGetMainCamera(out Camera mainCamera))
            {
                return;
            }

            Vector2 screenPosition = Input.mousePosition;
            Vector3 worldPosition = self.ScreenToWorldPosition(mainCamera, screenPosition);
            InputHitResult[] hitResults = BuildHitResults(worldPosition, self.PhysicsLayerMask);

            if (Input.GetMouseButtonDown(0))
            {
                self.BeginPress(screenPosition, worldPosition);
                PointerInputContext pointerContext = self.CreatePointerContext(screenPosition, worldPosition, hitResults);
                EventSystem.Instance.Publish(self.Scene(), new PointerDown() { Context = pointerContext });
            }

            if (self.IsPointerPressed && Input.GetMouseButton(0) && !Input.GetMouseButtonDown(0))
            {
                PointerInputContext pointerContext = self.CreatePointerContext(screenPosition, worldPosition, hitResults);
                EventSystem.Instance.Publish(self.Scene(), new PointerHold() { Context = pointerContext });
            }

            if (self.IsPointerPressed && Input.GetMouseButtonUp(0))
            {
                PointerInputContext pointerContext = self.CreatePointerContext(screenPosition, worldPosition, hitResults);
                EventSystem.Instance.Publish(self.Scene(), new PointerUp() { Context = pointerContext });

                if (self.IsClick(screenPosition))
                {
                    EventSystem.Instance.Publish(self.Scene(), new PointerClick() { Context = pointerContext });
                }

                self.ClearPress();
            }
        }

        /// <summary>
        /// 获取当前可用于输入转换的主相机。
        /// </summary>
        /// <param name="self">输入组件自身。</param>
        /// <param name="mainCamera">返回找到的主相机。</param>
        /// <returns>是否成功找到可用主相机。</returns>
        private static bool TryGetMainCamera(this InputComponent self, out Camera mainCamera)
        {
            mainCamera = self.MainCamera;
            if (mainCamera != null && mainCamera.isActiveAndEnabled)
            {
                return true;
            }

            mainCamera = Camera.main;
            self.MainCamera = mainCamera;
            return mainCamera != null;
        }

        /// <summary>
        /// 将当前屏幕坐标转换为二维交互使用的世界坐标。
        /// </summary>
        /// <param name="self">输入组件自身。</param>
        /// <param name="mainCamera">执行转换使用的主相机。</param>
        /// <param name="screenPosition">待转换的屏幕坐标。</param>
        /// <returns>转换后的世界坐标。</returns>
        private static Vector3 ScreenToWorldPosition(this InputComponent self, Camera mainCamera, Vector2 screenPosition)
        {
            Vector3 screenPoint = new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(mainCamera.transform.position.z));
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPoint);
            worldPosition.z = 0f;
            return worldPosition;
        }

        /// <summary>
        /// 记录一次新的按压开始状态。
        /// </summary>
        /// <param name="self">输入组件自身。</param>
        /// <param name="screenPosition">本次按压开始时的屏幕坐标。</param>
        /// <param name="worldPosition">本次按压开始时的世界坐标。</param>
        private static void BeginPress(this InputComponent self, Vector2 screenPosition, Vector3 worldPosition)
        {
            self.IsPointerPressed = true;
            self.PressScreenPosition = screenPosition;
            self.PressWorldPosition = worldPosition;
            self.PressStartTime = Time.unscaledTime;
        }

        /// <summary>
        /// 清理一次按压结束后的状态缓存。
        /// </summary>
        /// <param name="self">输入组件自身。</param>
        private static void ClearPress(this InputComponent self)
        {
            self.IsPointerPressed = false;
            self.PressScreenPosition = Vector2.zero;
            self.PressWorldPosition = Vector3.zero;
            self.PressStartTime = 0f;
        }

        /// <summary>
        /// 根据当前组件状态与命中结果组装通用输入上下文。
        /// </summary>
        /// <param name="self">输入组件自身。</param>
        /// <param name="screenPosition">当前屏幕坐标。</param>
        /// <param name="worldPosition">当前世界坐标。</param>
        /// <param name="hitResults">当前命中结果数组。</param>
        /// <returns>可直接发布给业务层的输入上下文。</returns>
        private static PointerInputContext CreatePointerContext(this InputComponent self, Vector2 screenPosition, Vector3 worldPosition, InputHitResult[] hitResults)
        {
            return new PointerInputContext()
            {
                Button = PointerButton.Left,
                ScreenPosition = screenPosition,
                WorldPosition = worldPosition,
                PressScreenPosition = self.PressScreenPosition,
                PressWorldPosition = self.PressWorldPosition,
                Duration = self.IsPointerPressed ? Time.unscaledTime - self.PressStartTime : 0f,
                HitResults = hitResults,
            };
        }

        /// <summary>
        /// 判断本次按压抬起是否满足点击阈值。
        /// </summary>
        /// <param name="self">输入组件自身。</param>
        /// <param name="releaseScreenPosition">抬起时的屏幕坐标。</param>
        /// <returns>是否应视为一次点击。</returns>
        private static bool IsClick(this InputComponent self, Vector2 releaseScreenPosition)
        {
            float duration = Time.unscaledTime - self.PressStartTime;
            float distance = Vector2.Distance(self.PressScreenPosition, releaseScreenPosition);
            return duration <= ClickDurationThreshold && distance <= ClickScreenDistanceThreshold;
        }

        /// <summary>
        /// 查询当前世界坐标下全部二维碰撞命中，并补齐 Unity 到 ET 的桥接信息。
        /// </summary>
        /// <param name="worldPosition">待检测的世界坐标。</param>
        /// <param name="physicsLayerMask">命中检测使用的 LayerMask。</param>
        /// <returns>当前坐标命中的全部结果数组。</returns>
        private static InputHitResult[] BuildHitResults(Vector3 worldPosition, int physicsLayerMask)
        {
            Collider2D[] colliders = Physics2D.OverlapPointAll(worldPosition, physicsLayerMask);
            if (colliders == null || colliders.Length == 0)
            {
                return Array.Empty<InputHitResult>();
            }

            InputHitResult[] hitResults = new InputHitResult[colliders.Length];
            for (int i = 0; i < colliders.Length; ++i)
            {
                Collider2D collider2D = colliders[i];
                GameObject gameObject = collider2D.gameObject;
                GameObjectEntityRef entityRef = gameObject.GetComponent<GameObjectEntityRef>();
                if (entityRef == null)
                {
                    entityRef = gameObject.GetComponentInParent<GameObjectEntityRef>();
                }

                hitResults[i] = new InputHitResult()
                {
                    Collider2D = collider2D,
                    GameObject = gameObject,
                    Transform = collider2D.transform,
                    Tag = gameObject.tag,
                    Entity = entityRef != null ? entityRef.Entity : null,
                };
            }

            return hitResults;
        }
    }
}
