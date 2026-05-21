using System;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    /// <summary>
    /// 单个存档槽位
    /// </summary>
    [ChildOf(typeof(SaveIndexComponent))]
    public partial class SaveSlot : Entity, IAwake<string, SaveSlotType>, ISerializeToEntity
    {
        /// <summary>
        /// 槽位标识
        /// </summary>
        [BsonElement]
        public string SlotId { get; set; } = string.Empty;

        /// <summary>
        /// 槽位类型
        /// </summary>
        [BsonElement]
        public SaveSlotType SlotType { get; set; } = SaveSlotType.Manual;

        /// <summary>
        /// 创建顺序
        /// </summary>
        [BsonElement]
        public int SortOrder { get; set; }

        /// <summary>
        /// 是否已加载完整数据
        /// </summary>
        [BsonIgnore]
        public bool IsDataLoaded { get; set; }

        /// <summary>
        /// 最后加载时间
        /// </summary>
        [BsonIgnore]
        public DateTime LastLoadedTime { get; set; }
    }
}
