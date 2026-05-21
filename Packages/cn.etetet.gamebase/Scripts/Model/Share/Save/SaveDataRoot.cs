using System;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    /// <summary>
    /// 单个槽位的完整存档数据根
    /// </summary>
    [ChildOf(typeof(SaveSlot))]
    public partial class SaveDataRoot : Entity, IAwake<string>, ISaveDataComponent
    {
        /// <summary>
        /// 槽位标识
        /// </summary>
        [BsonElement]
        public string SlotId { get; set; } = string.Empty;

        /// <summary>
        /// 数据版本
        /// </summary>
        [BsonElement]
        public int DataVersion { get; set; } = SaveConst.CurrentDataVersion;

        /// <summary>
        /// 创建时间
        /// </summary>
        [BsonElement]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后修改时间
        /// </summary>
        [BsonElement]
        public DateTime LastModifiedTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 存档数据类型标识
        /// </summary>
        [BsonIgnore]
        public string SaveDataType => nameof(SaveDataRoot);
    }
}
