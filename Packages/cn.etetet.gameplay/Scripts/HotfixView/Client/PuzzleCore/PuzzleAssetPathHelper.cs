namespace ET.Client
{
    /// <summary>
    /// PuzzleCore 的配置路径转 YooAssets 定位地址辅助方法。
    /// </summary>
    public static class PuzzleAssetPathHelper
    {
        private const string GameplayResourceRoot = "Packages/cn.etetet.gameplay/Resources/";
        private const string PrefabExtension = ".prefab";

        /// <summary>
        /// 将配置中的短 prefab 路径转换为 YooAssets 可直接加载的定位地址。
        /// </summary>
        /// <param name="prefabPath">配置中的 prefab 路径。</param>
        /// <returns>YooAssets 可加载的完整定位地址。</returns>
        public static string ToAssetLocation(string prefabPath)
        {
            if (string.IsNullOrWhiteSpace(prefabPath))
            {
                throw new System.ArgumentException("prefab path is empty", nameof(prefabPath));
            }

            string normalizedPath = prefabPath.Replace('\\', '/').TrimStart('/');
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
