using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// 存档编辑器菜单
    /// </summary>
    public static class SaveEditorMenu
    {
        private const string MenuRoot = "Tools/存档/";
        private const int MenuPriority = 2100;

        /// <summary>
        /// 打开当前客户端存档根目录
        /// </summary>
        [MenuItem(MenuRoot + "打开当前存档路径", false, MenuPriority)]
        private static void OpenCurrentSaveDirectory()
        {
            string saveRootDirectory = GetCurrentSaveRootDirectory();
            Directory.CreateDirectory(saveRootDirectory);
            EditorUtility.RevealInFinder(saveRootDirectory);
            Debug.Log($"已打开存档目录: {saveRootDirectory}");
        }

        /// <summary>
        /// 清空当前客户端存档根目录
        /// </summary>
        [MenuItem(MenuRoot + "清空存档", false, MenuPriority + 1)]
        private static void ClearCurrentSaveDirectory()
        {
            string saveRootDirectory = GetCurrentSaveRootDirectory();
            if (!Directory.Exists(saveRootDirectory))
            {
                Debug.Log($"存档目录不存在，无需清空: {saveRootDirectory}");
                return;
            }

            try
            {
                Directory.Delete(saveRootDirectory, true);
                Directory.CreateDirectory(saveRootDirectory);
                Debug.Log($"已清空存档目录: {saveRootDirectory}");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorUtility.DisplayDialog("清空存档失败", $"无法清空存档目录。\n{saveRootDirectory}\n{e.Message}", "确定");
            }
        }

        /// <summary>
        /// 获取当前客户端使用的存档根目录
        /// </summary>
        /// <returns>存档根目录</returns>
        private static string GetCurrentSaveRootDirectory()
        {
            return Path.Combine(Application.persistentDataPath, SaveConst.SaveDirectoryName);
        }
    }
}
