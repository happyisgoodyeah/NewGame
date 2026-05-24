namespace ET
{
    /// <summary>
    /// 字符串常用判断扩展
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// 判断字符串是否为空或长度为零
        /// </summary>
        /// <param name="self">字符串实例</param>
        /// <returns>是否为空或长度为零</returns>
        public static bool IsNullOrEmpty(this string self)
        {
            return string.IsNullOrEmpty(self);
        }

        /// <summary>
        /// 判断字符串是否为空或只包含空白字符
        /// </summary>
        /// <param name="self">字符串实例</param>
        /// <returns>是否为空或只包含空白字符</returns>
        public static bool IsNullOrWhiteSpace(this string self)
        {
            return string.IsNullOrWhiteSpace(self);
        }
    }
}
