using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    /// <summary>
    /// 关卡进度存档数据
    /// </summary>
    [ComponentOf(typeof(SaveDataRoot))]
    public partial class LevelProgressSaveDataComponent : Entity, IAwake, ISerializeToEntity, ISaveDataComponent
    {
        /// <summary>
        /// 数据版本
        /// </summary>
        [BsonElement]
        public int DataVersion { get; set; } = SaveConst.CurrentDataVersion;

        /// <summary>
        /// 已解锁关卡配置 id
        /// </summary>
        [BsonElement]
        public List<int> UnlockedLevelIds { get; set; } = new();

        /// <summary>
        /// 已通关关卡配置 id
        /// </summary>
        [BsonElement]
        public List<int> PassedLevelIds { get; set; } = new();

        /// <summary>
        /// 最后修改时间
        /// </summary>
        [BsonElement]
        public DateTime LastModifiedTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 存档数据类型标识
        /// </summary>
        [BsonIgnore]
        public string SaveDataType => nameof(LevelProgressSaveDataComponent);
    }
}
