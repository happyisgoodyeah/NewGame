using System;
using System.IO;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using Process = System.Diagnostics.Process;
using ProcessStartInfo = System.Diagnostics.ProcessStartInfo;

namespace ET
{
    public static class TortoiseGitMenu
    {
        private const string CustomTortoiseGitProcKey = "ET.TortoiseGitProc.Path";
        private const string MenuRoot = "ET/Git/";
        private const string TortoiseGitProcName = "TortoiseGitProc.exe";
        private const int GitMenuPriority = 2000;

        [MenuItem(MenuRoot + "Pull", false, GitMenuPriority)]
        private static void Pull()
        {
            ExecuteGitCommand("pull", "Pull");
        }

        [MenuItem(MenuRoot + "Pull", true)]
        private static bool ValidatePull()
        {
            return IsWindowsEditor() && HasGitRepository();
        }

        [Shortcut("ET/Git/Pull", KeyCode.F5)]
        private static void PullShortcut()
        {
            if (ValidatePull())
            {
                Pull();
            }
        }

        [MenuItem(MenuRoot + "Commit", false, GitMenuPriority + 1)]
        private static void Commit()
        {
            ExecuteGitCommand("commit", "Commit");
        }

        [MenuItem(MenuRoot + "Commit", true)]
        private static bool ValidateCommit()
        {
            return IsWindowsEditor() && HasGitRepository();
        }

        [Shortcut("ET/Git/Commit", KeyCode.F4)]
        private static void CommitShortcut()
        {
            if (ValidateCommit())
            {
                Commit();
            }
        }

        [MenuItem(MenuRoot + "Push", false, GitMenuPriority + 2)]
        private static void Push()
        {
            ExecuteGitCommand("push", "Push");
        }

        [MenuItem(MenuRoot + "Push", true)]
        private static bool ValidatePush()
        {
            return IsWindowsEditor() && HasGitRepository();
        }

        [Shortcut("ET/Git/Push", KeyCode.F5)]
        private static void PushShortcut()
        {
            if (ValidatePush())
            {
                Push();
            }
        }

        [MenuItem(MenuRoot + "Select TortoiseGitProc", false, GitMenuPriority + 20)]
        private static void SelectTortoiseGitProc()
        {
            string defaultDirectory = GetDefaultTortoiseGitDirectory();
            string selectedPath = EditorUtility.OpenFilePanel("选择 TortoiseGitProc.exe", defaultDirectory, "exe");
            if (string.IsNullOrEmpty(selectedPath))
            {
                return;
            }

            if (!string.Equals(Path.GetFileName(selectedPath), TortoiseGitProcName, StringComparison.OrdinalIgnoreCase))
            {
                EditorUtility.DisplayDialog("选择失败", "请选择 TortoiseGitProc.exe。", "确定");
                return;
            }

            EditorPrefs.SetString(CustomTortoiseGitProcKey, selectedPath);
            Debug.Log($"TortoiseGitProc 已设置为: {selectedPath}");
        }

        [MenuItem(MenuRoot + "Clear Custom TortoiseGitProc", false, GitMenuPriority + 21)]
        private static void ClearCustomTortoiseGitProc()
        {
            EditorPrefs.DeleteKey(CustomTortoiseGitProcKey);
            Debug.Log("已清除自定义 TortoiseGitProc 路径。");
        }

        private static void ExecuteGitCommand(string command, string displayName)
        {
            if (!TryGetProjectRoot(out string projectRoot))
            {
                EditorUtility.DisplayDialog("Git 命令不可用", "未找到当前 Unity 工程的 Git 仓库根目录。", "确定");
                return;
            }

            if (!TryGetTortoiseGitProcPath(out string exePath))
            {
                bool shouldSelect = EditorUtility.DisplayDialog(
                    "未找到 TortoiseGit",
                    "没有找到 TortoiseGitProc.exe。\n你可以先通过 ET/Git/Select TortoiseGitProc 指定安装路径。",
                    "立即选择",
                    "取消");

                if (shouldSelect)
                {
                    SelectTortoiseGitProc();
                }

                return;
            }

            string arguments = $"/command:{command} /path:\"{projectRoot}\" /closeonend:1";

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = arguments,
                    WorkingDirectory = projectRoot,
                    UseShellExecute = true,
                    CreateNoWindow = true,
                });
                Debug.Log($"{displayName} 已启动: {projectRoot}");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorUtility.DisplayDialog("启动失败", $"无法启动 TortoiseGit {displayName}。\n{e.Message}", "确定");
            }
        }

        private static bool TryGetProjectRoot(out string projectRoot)
        {
            DirectoryInfo directory = new DirectoryInfo(Path.GetFullPath(Path.Combine(Application.dataPath, "..")));

            while (directory != null)
            {
                string gitPath = Path.Combine(directory.FullName, ".git");
                if (Directory.Exists(gitPath) || File.Exists(gitPath))
                {
                    projectRoot = directory.FullName;
                    return true;
                }

                directory = directory.Parent;
            }

            projectRoot = string.Empty;
            return false;
        }

        private static bool HasGitRepository()
        {
            return TryGetProjectRoot(out _);
        }

        private static bool TryGetTortoiseGitProcPath(out string exePath)
        {
            string customPath = EditorPrefs.GetString(CustomTortoiseGitProcKey, string.Empty);
            if (!string.IsNullOrWhiteSpace(customPath) && File.Exists(customPath))
            {
                exePath = customPath;
                return true;
            }

            string[] candidatePaths =
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "TortoiseGit", "bin", TortoiseGitProcName),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "TortoiseGit", "bin", TortoiseGitProcName),
            };

            foreach (string candidatePath in candidatePaths)
            {
                if (File.Exists(candidatePath))
                {
                    exePath = candidatePath;
                    return true;
                }
            }

            string pathExecutable = FindExecutableInPath(TortoiseGitProcName);
            if (!string.IsNullOrEmpty(pathExecutable))
            {
                exePath = pathExecutable;
                return true;
            }

            exePath = string.Empty;
            return false;
        }

        private static string FindExecutableInPath(string executableName)
        {
            string environmentPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            string[] pathEntries = environmentPath.Split(Path.PathSeparator);

            foreach (string pathEntry in pathEntries)
            {
                if (string.IsNullOrWhiteSpace(pathEntry))
                {
                    continue;
                }

                string fullPath = Path.Combine(pathEntry.Trim().Trim('"'), executableName);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            return string.Empty;
        }

        private static string GetDefaultTortoiseGitDirectory()
        {
            string defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "TortoiseGit", "bin");
            if (Directory.Exists(defaultPath))
            {
                return defaultPath;
            }

            defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "TortoiseGit", "bin");
            if (Directory.Exists(defaultPath))
            {
                return defaultPath;
            }

            return Application.dataPath;
        }

        private static bool IsWindowsEditor()
        {
            return Application.platform == RuntimePlatform.WindowsEditor;
        }
    }
}
