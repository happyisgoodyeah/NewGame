using System;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    /// <summary>
    /// 第一阶段基础存档数据
    /// </summary>
    [ComponentOf(typeof(SaveDataRoot))]
    public partial class BasicSaveDataComponent : Entity, IAwake, ISerializeToEntity, ISaveDataComponent
    {
        /// <summary>
        /// 数据版本
        /// </summary>
        [BsonElement]
        public int DataVersion { get; set; } = SaveConst.CurrentDataVersion;

        /// <summary>
        /// 玩家标识
        /// </summary>
        [BsonElement]
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 玩家名称
        /// </summary>
        [BsonElement]
        public string PlayerName { get; set; } = "New Player";

        /// <summary>
        /// 当前场景名
        /// </summary>
        [BsonElement]
        public string SceneName { get; set; } = string.Empty;

        /// <summary>
        /// 简单进度值
        /// </summary>
        [BsonElement]
        public int Progress { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        [BsonElement]
        public DateTime LastModifiedTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 存档数据类型标识
        /// </summary>
        [BsonIgnore]
        public string SaveDataType => nameof(BasicSaveDataComponent);
    }
}
