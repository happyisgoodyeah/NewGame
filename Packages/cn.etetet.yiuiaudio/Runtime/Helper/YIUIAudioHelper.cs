using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.Utilities;

namespace YIUIFramework
{
    public static class YIUIAudioHelper
    {
        private static List<string> m_AllKey;

        private static List<string> AllRedDotKey
        {
            get
            {
                if (m_AllKey == null)
                {
                    GetKeys();
                }

                return m_AllKey;
            }
        }

        private static HashSet<string> m_HashAllKey;

        /// <summary>
        /// 检查这个Key 是否存在
        /// </summary>
        public static bool ContainsKey(string key)
        {
            if (m_HashAllKey == null)
            {
                m_HashAllKey = new();
                m_HashAllKey.AddRange(AllRedDotKey);
            }

            return m_HashAllKey.Contains(key);
        }

        /// <summary>
        /// 反射获取枚举列表
        /// 注意: 运行时使用的是ET编译过的dll
        /// 如果新增了没有编译 则无法获取到最新数据 请注意一定要编译后运行
        /// </summary>
        public static List<string> GetKeys(bool force = false)
        {
            if (m_AllKey != null && !force)
            {
                return m_AllKey;
            }

            m_AllKey = new();
            m_HashAllKey = null;
            #if UNITY_EDITOR
            m_KeyDesc = new();
            #endif

            var assembly = AssemblyHelper.GetAssembly("ET.Model");
            if (assembly == null)
            {
                Logger.LogError($"没有找到ET.Model程序集");
                return m_AllKey;
            }

            Type audioKeysType = assembly.GetType("ET.EAudioKeys");
            if (audioKeysType == null)
            {
                Logger.LogError($"没有找到ET.EAudioKeys 类型");
                return m_AllKey;
            }

            var fields = audioKeysType.GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var field in fields)
            {
                var key = field.Name;
                if (key == "None")
                {
                    continue;
                }

                m_AllKey.Add(key);

                #if UNITY_EDITOR
                var labelAttr = field.GetCustomAttribute<LabelTextAttribute>();
                var value = labelAttr?.Text ?? string.Empty;

                m_KeyDesc[key] = value;
                #endif
            }

            return m_AllKey;
        }

        #if UNITY_EDITOR
        private static Dictionary<string, string> m_KeyDesc;

        // Editor 时可用
        internal static string GetDesc(string key)
        {
            return m_KeyDesc?.GetValueOrDefault(key, "");
        }

        // 统一显示描述
        internal static string GetDisplayDesc(string key)
        {
            return $"{key}_{m_KeyDesc?.GetValueOrDefault(key, "")}";
        }

        private const int MAX_GROUP_SIZE = 10;

        /// <summary>
        /// 超过 MAX_GROUP_SIZE 个条目时，按字母顺序分组显示
        /// </summary>
        internal static ValueDropdownList<string> SubDisplayValueDropdownList(ValueDropdownList<string> keys)
        {
            // 按字符串升序排序
            keys.Sort((a, b) => string.Compare(a.Text, b.Text, StringComparison.Ordinal));

            if (keys.Count <= MAX_GROUP_SIZE)
            {
                return keys;
            }

            return GetGroupValueDropdownList(keys, MAX_GROUP_SIZE);
        }

        /// <summary>
        /// 递归分组
        /// </summary>
        private static ValueDropdownList<string> GetGroupValueDropdownList(ValueDropdownList<string> items, int groupSize)
        {
            ValueDropdownList<string> allGroups = new ValueDropdownList<string>();
            int count = items.Count;

            while (count > 0)
            {
                int groupCount = Math.Min(groupSize, count);

                var first = items[0];
                var last = items[groupCount - 1];

                string groupText = $"{first.Text} - {last.Text}";

                for (int i = 0; i < groupCount; i++)
                {
                    var item = items[0];
                    allGroups.Add(new ValueDropdownItem<string>
                    {
                        Text = $"{groupText}/{item.Text}",
                        Value = item.Value
                    });
                    items.RemoveAt(0);
                }

                count -= groupCount;
            }

            int nextGroupSize = groupSize * MAX_GROUP_SIZE;
            if (allGroups.Count > nextGroupSize)
            {
                return GetGroupValueDropdownList(allGroups, nextGroupSize);
            }

            return allGroups;
        }
        #endif

        public static string GetConfigAudioName(string key, int index = 0)
        {
            var assembly = AssemblyHelper.GetAssembly("ET.Model");
            if (assembly == null)
            {
                Logger.LogError($"没有找到ET.Model程序集");
                return null;
            }

            Type helperType = assembly.GetType("ET.AudioConfigHelper");
            if (helperType == null)
            {
                Logger.LogError("找不到类型 ET.AudioConfigHelper");
                return null;
            }

            var method = helperType.GetMethod("GetAudioName",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new Type[] { typeof(string), typeof(int) },
                null);

            if (method == null)
            {
                Logger.LogError("找不到 AudioConfigHelper.GetAudioName 方法");
                return null;
            }

            try
            {
                return (string)method.Invoke(null, new object[] { key, index });
            }
            catch (Exception ex)
            {
                Logger.LogError($"反射调用 AudioConfigHelper.GetAudioName 异常: {ex}");
                return null;
            }
        }
    }
}