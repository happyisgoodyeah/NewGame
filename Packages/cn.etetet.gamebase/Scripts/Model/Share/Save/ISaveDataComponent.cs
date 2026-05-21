using System;

namespace ET
{
    /// <summary>
    /// 存档数据组件标记接口
    /// </summary>
    public interface ISaveDataComponent
    {
        /// <summary>
        /// 数据组件类型标识
        /// </summary>
        string SaveDataType { get; }

        /// <summary>
        /// 数据版本
        /// </summary>
        int DataVersion { get; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        DateTime LastModifiedTime { get; set; }
    }
}
