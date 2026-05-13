namespace ET.Client
{
    /// <summary>
    /// 项目资源定位地址转换辅助方法。
    /// </summary>
    public static class ResourceLocationHelper
    {
        private const string GameplayResourceRoot = "Packages/cn.etetet.gameplay/Resources/";
        private const string PrefabExtension = ".prefab";

        /// <summary>
        /// 将资源配置中的相对路径转换为 YooAssets 可加载的定位地址。
        /// </summary>
        /// <param name="resourcePath">资源配置中的路径。</param>
        /// <returns>YooAssets 可加载的完整定位地址。</returns>
        public static string ToAssetLocation(string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                throw new System.ArgumentException("resource path is empty", nameof(resourcePath));
            }

            string normalizedPath = resourcePath.Replace('\\', '/').TrimStart('/');
            if (normalizedPath.StartsWith(GameplayResourceRoot))
            {
                return normalizedPath.EndsWith(PrefabExtension) ? normalizedPath : $"{normalizedPath}{PrefabExtension}";
            }

            return normalizedPath.EndsWith(PrefabExtension)
                    ? $"{GameplayResourceRoot}{normalizedPath}"
                    : $"{GameplayResourceRoot}{normalizedPath}{PrefabExtension}";
        }
    }
}
