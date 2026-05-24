namespace ET
{
    [SaveValidation(typeof(LevelProgressSaveDataComponent))]
    public class LevelProgressSaveDataValidationHandler : ISaveValidationHandler
    {
        /// <summary>
        /// 校验关卡进度存档数据
        /// </summary>
        /// <param name="entity">待校验实体</param>
        /// <returns>是否通过校验</returns>
        public bool Handle(Entity entity)
        {
            if (entity is not LevelProgressSaveDataComponent progressData)
            {
                return false;
            }

            if (progressData.DataVersion <= 0)
            {
                Log.Error("关卡进度存档数据版本无效");
                return false;
            }

            if (progressData.UnlockedLevelIds == null)
            {
                Log.Error("已解锁关卡列表为空");
                return false;
            }

            if (progressData.PassedLevelIds == null)
            {
                Log.Error("已通关关卡列表为空");
                return false;
            }

            return true;
        }
    }
}
