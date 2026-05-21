using System;
using System.Collections.Generic;

namespace ET
{
    [EntitySystemOf(typeof(SaveValidationDispatcherComponent))]
    public static partial class SaveValidationDispatcherComponentSystem
    {
        /// <summary>
        /// 初始化校验处理器映射
        /// </summary>
        /// <param name="self">校验分发组件</param>
        [EntitySystem]
        private static void Awake(this SaveValidationDispatcherComponent self)
        {
            self.ValidationHandlers = new Dictionary<Type, ISaveValidationHandler>();
            self.LoadValidationHandlers();
        }

        /// <summary>
        /// 加载所有存档校验处理器
        /// </summary>
        /// <param name="self">校验分发组件</param>
        public static void LoadValidationHandlers(this SaveValidationDispatcherComponent self)
        {
            foreach (Type type in CodeTypes.Instance.GetTypes(typeof(SaveValidationAttribute)))
            {
                object[] attributes = type.GetCustomAttributes(typeof(SaveValidationAttribute), false);
                if (attributes.Length == 0)
                {
                    continue;
                }

                if (Activator.CreateInstance(type) is not ISaveValidationHandler handler)
                {
                    continue;
                }

                SaveValidationAttribute attribute = (SaveValidationAttribute)attributes[0];
                self.ValidationHandlers[attribute.EntityType] = handler;
            }
        }

        /// <summary>
        /// 校验单个存档实体
        /// </summary>
        /// <param name="self">校验分发组件</param>
        /// <param name="entity">待校验实体</param>
        /// <returns>是否通过校验</returns>
        public static bool ValidateEntity(this SaveValidationDispatcherComponent self, Entity entity)
        {
            if (entity == null)
            {
                return false;
            }

            Type entityType = entity.GetType();
            if (self.ValidationHandlers.TryGetValue(entityType, out ISaveValidationHandler handler))
            {
                try
                {
                    return handler.Handle(entity);
                }
                catch (Exception e)
                {
                    Log.Error($"存档实体校验异常: {entityType.Name} {e}");
                    return false;
                }
            }

            if (entity is ISaveDataComponent saveDataComponent && saveDataComponent.DataVersion <= 0)
            {
                Log.Error($"存档数据版本无效: {entityType.Name}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 递归校验存档实体树
        /// </summary>
        /// <param name="self">校验分发组件</param>
        /// <param name="entity">待校验实体树根</param>
        /// <returns>是否通过校验</returns>
        public static bool ValidateEntityTree(this SaveValidationDispatcherComponent self, Entity entity)
        {
            if (!self.ValidateEntity(entity))
            {
                return false;
            }

            if (entity.ComponentsCount() > 0)
            {
                foreach (Entity component in entity.Components.Values)
                {
                    if (!self.ValidateEntityTree(component))
                    {
                        return false;
                    }
                }
            }

            if (entity.ChildrenCount() > 0)
            {
                foreach (Entity child in entity.Children.Values)
                {
                    if (!self.ValidateEntityTree(child))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
