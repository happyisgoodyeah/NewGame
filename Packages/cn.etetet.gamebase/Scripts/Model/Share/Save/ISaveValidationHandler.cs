namespace ET
{
    /// <summary>
    /// 存档数据校验处理器接口
    /// </summary>
    public interface ISaveValidationHandler
    {
        /// <summary>
        /// 校验指定存档实体
        /// </summary>
        /// <param name="entity">待校验实体</param>
        /// <returns>是否通过校验</returns>
        bool Handle(Entity entity);
    }
}
