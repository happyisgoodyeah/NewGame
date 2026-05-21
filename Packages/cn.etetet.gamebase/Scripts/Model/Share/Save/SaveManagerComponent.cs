using System;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    /// <summary>
    /// 存档管理器组件
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class SaveManagerComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 存档根目录
        /// </summary>
        [BsonIgnore]
        public string SaveRootDirectory { get; set; } = string.Empty;

        /// <summary>
        /// 当前游戏版本
        /// </summary>
        [BsonIgnore]
        public string GameVersion { get; set; } = string.Empty;

        /// <summary>
        /// 当前存档槽位
        /// </summary>
        [BsonIgnore]
        public string CurrentSlotId { get; set; } = string.Empty;

        /// <summary>
        /// 是否已经完成初始化
        /// </summary>
        [BsonIgnore]
        public bool IsInitialized { get; set; }

        /// <summary>
        /// 初始化完成时间
        /// </summary>
        [BsonIgnore]
        public DateTime InitializedTime { get; set; }
    }
}
