using System.IO;
using UnityEngine;

namespace ET.Client
{
    public static class SaveManagerComponentClientSystem
    {
        /// <summary>
        /// 使用客户端持久化目录初始化存档管理器
        /// </summary>
        /// <param name="self">存档管理器</param>
        /// <returns>初始化结果</returns>
        public static async ETTask<SaveResult> InitializeClientAsync(this SaveManagerComponent self)
        {
            string saveRootDirectory = Path.Combine(Application.persistentDataPath, SaveConst.SaveDirectoryName);
            return await self.Initialize(saveRootDirectory, Application.version);
        }
    }
}
