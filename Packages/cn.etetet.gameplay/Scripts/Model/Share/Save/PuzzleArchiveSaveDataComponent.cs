using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    /// <summary>
    /// 拼图图鉴存档数据
    /// </summary>
    [ComponentOf(typeof(SaveDataRoot))]
    public partial class PuzzleArchiveSaveDataComponent : Entity, IAwake, ISerializeToEntity, ISaveDataComponent
    {
        /// <summary>
        /// 数据版本
        /// </summary>
        [BsonElement]
        public int DataVersion { get; set; } = SaveConst.CurrentDataVersion;

        /// <summary>
        /// 已解锁拼图配置 id
        /// </summary>
        [BsonElement]
        public List<int> UnlockedPuzzleIds { get; set; } = new();

        /// <summary>
        /// 最后修改时间
        /// </summary>
        [BsonElement]
        public DateTime LastModifiedTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 存档数据类型标识
        /// </summary>
        [BsonIgnore]
        public string SaveDataType => nameof(PuzzleArchiveSaveDataComponent);
    }
}
