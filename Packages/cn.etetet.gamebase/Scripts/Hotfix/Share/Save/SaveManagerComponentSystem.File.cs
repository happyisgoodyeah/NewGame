using System;
using System.IO;

namespace ET
{
    public static partial class SaveManagerComponentSystem
    {
        /// <summary>
        /// 加载存档索引
        /// </summary>
        /// <param name="self">存档管理器</param>
        public static async ETTask LoadIndexAsync(this SaveManagerComponent self)
        {
            Directory.CreateDirectory(self.SaveRootDirectory);

            string indexPath = self.GetIndexPath();
            if (File.Exists(indexPath))
            {
                try
                {
                    string json = await File.ReadAllTextAsync(indexPath);
                    SaveIndexComponent saveIndex = (SaveIndexComponent)MongoHelper.FromJson(typeof(SaveIndexComponent), json);
                    self.AttachIndex(saveIndex);
                    return;
                }
                catch (Exception e)
                {
                    Log.Error($"读取存档索引失败: {e}");
                }
            }

            self.CreateEmptyIndex();
            await self.RebuildIndexFromHeadersAsync();
            await self.SaveIndexAsync();
        }

        /// <summary>
        /// 保存存档索引
        /// </summary>
        /// <param name="self">存档管理器</param>
        public static async ETTask SaveIndexAsync(this SaveManagerComponent self)
        {
            SaveIndexComponent saveIndex = self.GetIndex();
            if (saveIndex == null)
            {
                return;
            }

            saveIndex.UpdateTime = DateTime.Now;
            Directory.CreateDirectory(self.SaveRootDirectory);

            string json = MongoHelper.ToJson(saveIndex);
            await File.WriteAllTextAsync(self.GetIndexPath(), json);
        }

        /// <summary>
        /// 写入槽位头信息与完整数据
        /// </summary>
        /// <param name="self">存档管理器</param>
        /// <param name="saveSlot">存档槽位</param>
        /// <param name="saveDataRoot">完整存档数据根</param>
        /// <returns>写入结果</returns>
        public static async ETTask<SaveResult> WriteSlotAsync(this SaveManagerComponent self, SaveSlot saveSlot, SaveDataRoot saveDataRoot)
        {
            if (saveSlot == null || saveDataRoot == null)
            {
                return SaveResult.Failed;
            }

            SaveDataHeaderComponent header = saveSlot.GetHeader();
            SaveValidationDispatcherComponent dispatcher = self.GetValidationDispatcher();
            if (!dispatcher.ValidateEntityTree(header) || !dispatcher.ValidateEntityTree(saveDataRoot))
            {
                return SaveResult.ValidationFailed;
            }

            saveDataRoot.MarkModified();

            byte[] data = MongoHelper.Serialize(saveDataRoot);
            header.DataSize = data.Length;
            header.GameVersion = self.GameVersion;
            header.UpdateTime = DateTime.Now;
            header.LastModifiedTime = header.UpdateTime;

            string slotDirectory = self.GetSlotDirectory(saveSlot.SlotId);
            Directory.CreateDirectory(slotDirectory);

            await File.WriteAllBytesAsync(self.GetDataPath(saveSlot.SlotId), data);
            await self.SaveHeaderAsync(saveSlot);
            await self.SaveIndexAsync();

            return SaveResult.Success;
        }

        /// <summary>
        /// 读取槽位完整数据
        /// </summary>
        /// <param name="self">存档管理器</param>
        /// <param name="saveSlot">存档槽位</param>
        /// <returns>完整存档数据根</returns>
        public static async ETTask<SaveDataRoot> ReadDataRootAsync(this SaveManagerComponent self, SaveSlot saveSlot)
        {
            string dataPath = self.GetDataPath(saveSlot.SlotId);
            if (!File.Exists(dataPath))
            {
                Log.Error($"存档数据文件不存在: {dataPath}");
                return null;
            }

            byte[] data = await File.ReadAllBytesAsync(dataPath);
            SaveDataRoot saveDataRoot = (SaveDataRoot)MongoHelper.Deserialize(typeof(SaveDataRoot), data);

            SaveValidationDispatcherComponent dispatcher = self.GetValidationDispatcher();
            if (!dispatcher.ValidateEntityTree(saveDataRoot))
            {
                saveDataRoot.Dispose();
                return null;
            }

            return saveDataRoot;
        }

        /// <summary>
        /// 保存槽位头信息
        /// </summary>
        /// <param name="self">存档管理器</param>
        /// <param name="saveSlot">存档槽位</param>
        private static async ETTask SaveHeaderAsync(this SaveManagerComponent self, SaveSlot saveSlot)
        {
            SaveDataHeaderComponent header = saveSlot.GetHeader();
            string json = MongoHelper.ToJson(header);
            await File.WriteAllTextAsync(self.GetHeaderPath(saveSlot.SlotId), json);
        }

        /// <summary>
        /// 从独立头文件重建索引
        /// </summary>
        /// <param name="self">存档管理器</param>
        private static async ETTask RebuildIndexFromHeadersAsync(this SaveManagerComponent self)
        {
            SaveIndexComponent saveIndex = self.GetIndex();
            foreach (string slotDirectory in Directory.GetDirectories(self.SaveRootDirectory))
            {
                string headerPath = Path.Combine(slotDirectory, SaveConst.HeaderFileName);
                if (!File.Exists(headerPath))
                {
                    continue;
                }

                try
                {
                    string json = await File.ReadAllTextAsync(headerPath);
                    SaveDataHeaderComponent header = (SaveDataHeaderComponent)MongoHelper.FromJson(typeof(SaveDataHeaderComponent), json);
                    if (string.IsNullOrWhiteSpace(header.SlotId) || saveIndex.GetSlot(header.SlotId) != null)
                    {
                        continue;
                    }

                    SaveSlot saveSlot = saveIndex.AddChild<SaveSlot, string, SaveSlotType>(header.SlotId, header.SlotType);
                    saveSlot.CopyHeaderFrom(header);
                }
                catch (Exception e)
                {
                    Log.Error($"读取存档头失败: {headerPath} {e}");
                }
            }
        }

        /// <summary>
        /// 创建空索引组件
        /// </summary>
        /// <param name="self">存档管理器</param>
        private static void CreateEmptyIndex(this SaveManagerComponent self)
        {
            if (self.GetIndex() != null)
            {
                self.RemoveComponent<SaveIndexComponent>();
            }

            self.AddComponent<SaveIndexComponent>();
        }

        /// <summary>
        /// 挂载反序列化得到的索引组件
        /// </summary>
        /// <param name="self">存档管理器</param>
        /// <param name="saveIndex">存档索引</param>
        private static void AttachIndex(this SaveManagerComponent self, SaveIndexComponent saveIndex)
        {
            if (self.GetIndex() != null)
            {
                self.RemoveComponent<SaveIndexComponent>();
            }

            self.AddComponent(saveIndex);
        }

        /// <summary>
        /// 获取索引文件路径
        /// </summary>
        /// <param name="self">存档管理器</param>
        /// <returns>索引文件路径</returns>
        private static string GetIndexPath(this SaveManagerComponent self)
        {
            return Path.Combine(self.SaveRootDirectory, SaveConst.IndexFileName);
        }

        /// <summary>
        /// 获取槽位目录
        /// </summary>
        /// <param name="self">存档管理器</param>
        /// <param name="slotId">槽位标识</param>
        /// <returns>槽位目录</returns>
        private static string GetSlotDirectory(this SaveManagerComponent self, string slotId)
        {
            return Path.Combine(self.SaveRootDirectory, slotId);
        }

        /// <summary>
        /// 获取槽位头文件路径
        /// </summary>
        /// <param name="self">存档管理器</param>
        /// <param name="slotId">槽位标识</param>
        /// <returns>槽位头文件路径</returns>
        private static string GetHeaderPath(this SaveManagerComponent self, string slotId)
        {
            return Path.Combine(self.GetSlotDirectory(slotId), SaveConst.HeaderFileName);
        }

        /// <summary>
        /// 获取槽位数据文件路径
        /// </summary>
        /// <param name="self">存档管理器</param>
        /// <param name="slotId">槽位标识</param>
        /// <returns>槽位数据文件路径</returns>
        private static string GetDataPath(this SaveManagerComponent self, string slotId)
        {
            return Path.Combine(self.GetSlotDirectory(slotId), SaveConst.DataFileName);
        }

        /// <summary>
        /// 删除槽位目录
        /// </summary>
        /// <param name="self">存档管理器</param>
        /// <param name="slotDirectory">槽位目录</param>
        private static void DeleteSlotDirectory(this SaveManagerComponent self, string slotDirectory)
        {
            if (!Directory.Exists(slotDirectory))
            {
                return;
            }

            Directory.Delete(slotDirectory, true);
        }
    }
}
