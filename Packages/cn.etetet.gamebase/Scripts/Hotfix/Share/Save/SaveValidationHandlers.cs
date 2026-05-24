using System;

namespace ET
{
    [SaveValidation(typeof(SaveDataHeaderComponent))]
    public class SaveDataHeaderValidationHandler : ISaveValidationHandler
    {
        /// <summary>
        /// 校验存档头信息
        /// </summary>
        /// <param name="entity">待校验实体</param>
        /// <returns>是否通过校验</returns>
        public bool Handle(Entity entity)
        {
            if (entity is not SaveDataHeaderComponent header)
            {
                return false;
            }

            if (header.SlotId.IsNullOrWhiteSpace())
            {
                Log.Error("存档头槽位标识为空");
                return false;
            }

            if (header.DataVersion <= 0)
            {
                Log.Error("存档头数据版本无效");
                return false;
            }

            if (header.CreateTime > DateTime.Now || header.UpdateTime > DateTime.Now)
            {
                Log.Error("存档头时间异常");
                return false;
            }

            return true;
        }
    }

    [SaveValidation(typeof(SaveDataRoot))]
    public class SaveDataRootValidationHandler : ISaveValidationHandler
    {
        /// <summary>
        /// 校验完整存档数据根
        /// </summary>
        /// <param name="entity">待校验实体</param>
        /// <returns>是否通过校验</returns>
        public bool Handle(Entity entity)
        {
            if (entity is not SaveDataRoot saveDataRoot)
            {
                return false;
            }

            if (saveDataRoot.SlotId.IsNullOrWhiteSpace())
            {
                Log.Error("存档数据根槽位标识为空");
                return false;
            }

            if (saveDataRoot.DataVersion <= 0)
            {
                Log.Error("存档数据根版本无效");
                return false;
            }

            return true;
        }
    }
}
