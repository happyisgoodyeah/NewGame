using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    /// <summary>
    /// 存档校验分发组件
    /// </summary>
    [ComponentOf(typeof(SaveManagerComponent))]
    public class SaveValidationDispatcherComponent : Entity, IAwake
    {
        /// <summary>
        /// 实体类型到校验处理器的映射
        /// </summary>
        [BsonIgnore]
        public Dictionary<Type, ISaveValidationHandler> ValidationHandlers = new();
    }
}
