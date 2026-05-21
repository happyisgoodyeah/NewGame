using System;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    /// <summary>
    /// 存档头信息
    /// </summary>
    [ComponentOf(typeof(SaveSlot))]
    public partial class SaveDataHeaderComponent : Entity, IAwake, ISerializeToEntity, ISaveDataComponent
    {
        /// <summary>
        /// 槽位标识
        /// </summary>
        [BsonElement]
        public string SlotId { get; set; } = string.Empty;

        /// <summary>
        /// 展示名称
        /// </summary>
        [BsonElement]
        public string DisplayName { get; set; } = SaveConst.DefaultDisplayName;

        /// <summary>
        /// 槽位类型
        /// </summary>
        [BsonElement]
        public SaveSlotType SlotType { get; set; } = SaveSlotType.Manual;

        /// <summary>
        /// 数据版本
        /// </summary>
        [BsonElement]
        public int DataVersion { get; set; } = SaveConst.CurrentDataVersion;

        /// <summary>
        /// 游戏版本
        /// </summary>
        [BsonElement]
        public string GameVersion { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        [BsonElement]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后保存时间
        /// </summary>
        [BsonElement]
        public DateTime UpdateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 游玩时长秒数
        /// </summary>
        [BsonElement]
        public double PlayTimeSeconds { get; set; }

        /// <summary>
        /// 数据文件大小
        /// </summary>
        [BsonElement]
        public long DataSize { get; set; }

        /// <summary>
        /// 描述文本
        /// </summary>
        [BsonElement]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 存档数据类型标识
        /// </summary>
        [BsonIgnore]
        public string SaveDataType => nameof(SaveDataHeaderComponent);

        /// <summary>
        /// 最后修改时间
        /// </summary>
        [BsonIgnore]
        public DateTime LastModifiedTime
        {
            get => this.UpdateTime;
            set => this.UpdateTime = value;
        }
    }
}
