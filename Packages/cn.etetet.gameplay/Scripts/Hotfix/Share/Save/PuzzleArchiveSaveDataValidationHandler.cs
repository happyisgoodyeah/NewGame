namespace ET
{
    [SaveValidation(typeof(PuzzleArchiveSaveDataComponent))]
    public class PuzzleArchiveSaveDataValidationHandler : ISaveValidationHandler
    {
        /// <summary>
        /// 校验拼图图鉴存档数据
        /// </summary>
        /// <param name="entity">待校验实体</param>
        /// <returns>是否通过校验</returns>
        public bool Handle(Entity entity)
        {
            if (entity is not PuzzleArchiveSaveDataComponent archiveData)
            {
                return false;
            }

            if (archiveData.DataVersion <= 0)
            {
                Log.Error("拼图图鉴存档数据版本无效");
                return false;
            }

            if (archiveData.UnlockedPuzzleIds == null)
            {
                Log.Error("已解锁拼图列表为空");
                return false;
            }

            return true;
        }
    }
}
