using System;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    /// <summary>
    /// 存档索引数据
    /// </summary>
    [ComponentOf(typeof(SaveManagerComponent))]
    public partial class SaveIndexComponent : Entity, IAwake, ISerializeToEntity
    {
        /// <summary>
        /// 索引数据版本
        /// </summary>
        [BsonElement]
        public int DataVersion { get; set; } = SaveConst.CurrentDataVersion;

        /// <summary>
        /// 当前选中的槽位
        /// </summary>
        [BsonElement]
        public string CurrentSlotId { get; set; } = string.Empty;

        /// <summary>
        /// 索引创建时间
        /// </summary>
        [BsonElement]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 索引更新时间
        /// </summary>
        [BsonElement]
        public DateTime UpdateTime { get; set; } = DateTime.Now;
    }
}
