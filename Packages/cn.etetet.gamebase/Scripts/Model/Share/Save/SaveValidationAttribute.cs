using System;

namespace ET
{
    /// <summary>
    /// 标记存档实体对应的校验处理器
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SaveValidationAttribute : BaseAttribute
    {
        /// <summary>
        /// 被校验的实体类型
        /// </summary>
        public Type EntityType { get; }

        /// <summary>
        /// 创建存档校验处理器标记
        /// </summary>
        /// <param name="entityType">被校验的实体类型</param>
        public SaveValidationAttribute(Type entityType)
        {
            this.EntityType = entityType;
        }
    }
}
