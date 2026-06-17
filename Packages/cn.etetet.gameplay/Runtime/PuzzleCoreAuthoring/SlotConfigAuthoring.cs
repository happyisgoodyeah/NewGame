using System;
using System.Globalization;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ET.Client
{
    /// <summary>
    /// Slot 预制体配置导出标记
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SlotConfigAuthoring : MonoBehaviour
    {
        [SerializeField]
        private int configId;

        [SerializeField]
        private string configName;

        [SerializeField]
        private int x = 1;

        [SerializeField]
        private int y = 1;

        [SerializeField]
        private string prefabPathOverride;

        [SerializeField]
        private bool allowPlace = true;

        [SerializeField]
        private bool includeLubanMarkerColumn = true;

        /// <summary>
        /// 根据当前字段生成可粘贴到 Slot.xlsx 的制表符行
        /// </summary>
        /// <param name="resolvedPrefabPath">已解析的 Slot 资源相对路径</param>
        /// <returns>可直接复制到 Luban 表的行文本</returns>
        public string BuildLubanRow(string resolvedPrefabPath)
        {
            this.ValidateConfig(resolvedPrefabPath);

            string[] columns = includeLubanMarkerColumn
                    ? new[]
                    {
                            string.Empty,
                            FormatInt(configId),
                            NormalizeCell(configName),
                            FormatInt(x),
                            FormatInt(y),
                            NormalizeCell(resolvedPrefabPath),
                            FormatBool(allowPlace),
                    }
                    : new[]
                    {
                            FormatInt(configId),
                            NormalizeCell(configName),
                            FormatInt(x),
                            FormatInt(y),
                            NormalizeCell(resolvedPrefabPath),
                            FormatBool(allowPlace),
                    };

            return string.Join("\t", columns);
        }

        /// <summary>
        /// 获取当前 Slot 配置 id
        /// </summary>
        /// <returns>Slot 配置 id</returns>
        public int GetConfigId()
        {
            return configId;
        }

        /// <summary>
        /// 获取当前 Slot 配置名
        /// </summary>
        /// <returns>Slot 配置名</returns>
        public string GetConfigName()
        {
            return configName;
        }

        /// <summary>
        /// 获取手动覆盖的资源相对路径
        /// </summary>
        /// <returns>手动覆盖路径</returns>
        public string GetPrefabPathOverride()
        {
            return prefabPathOverride;
        }

        /// <summary>
        /// 用推断值填充尚未设置的配置字段
        /// </summary>
        /// <param name="inferredConfigId">从预制体名推断出的配置 id</param>
        /// <param name="inferredPrefabPath">从资源路径推断出的配置路径</param>
        public void ApplyInferredDefaults(int inferredConfigId, string inferredPrefabPath)
        {
            if (configId <= 0 && inferredConfigId > 0)
            {
                configId = inferredConfigId;
            }

            if (string.IsNullOrWhiteSpace(configName))
            {
                configName = this.gameObject.name;
            }

            if (string.IsNullOrWhiteSpace(prefabPathOverride))
            {
                prefabPathOverride = inferredPrefabPath;
            }
        }

        /// <summary>
        /// 编辑器重置时按对象名补齐易推断字段
        /// </summary>
        private void Reset()
        {
            configId = ParseTrailingNumber(this.gameObject.name);
            configName = this.gameObject.name;
            x = 1;
            y = 1;
            allowPlace = true;
            includeLubanMarkerColumn = true;
        }

        /// <summary>
        /// 校验当前字段是否足以生成 Slot 配置
        /// </summary>
        /// <param name="resolvedPrefabPath">已解析的 Slot 资源相对路径</param>
        private void ValidateConfig(string resolvedPrefabPath)
        {
            if (configId <= 0)
            {
                throw new InvalidOperationException($"{nameof(SlotConfigAuthoring)} config id is invalid: {configId}");
            }

            if (string.IsNullOrWhiteSpace(configName))
            {
                throw new InvalidOperationException($"{nameof(SlotConfigAuthoring)} config name is empty");
            }

            if (x <= 0 || y <= 0)
            {
                throw new InvalidOperationException($"{nameof(SlotConfigAuthoring)} size is invalid: {x}x{y}");
            }

            if (string.IsNullOrWhiteSpace(resolvedPrefabPath))
            {
                throw new InvalidOperationException($"{nameof(SlotConfigAuthoring)} prefab path is empty");
            }
        }

        /// <summary>
        /// 将单元格文本规整为单行内容
        /// </summary>
        /// <param name="value">原始文本</param>
        /// <returns>规整后的单元格文本</returns>
        private static string NormalizeCell(string value)
        {
            return value?.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ').Trim() ?? string.Empty;
        }

        /// <summary>
        /// 按无区域格式输出整数
        /// </summary>
        /// <param name="value">整数值</param>
        /// <returns>整数文本</returns>
        private static string FormatInt(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 按 Excel 可识别格式输出布尔值
        /// </summary>
        /// <param name="value">布尔值</param>
        /// <returns>布尔文本</returns>
        private static string FormatBool(bool value)
        {
            return value ? "TRUE" : "FALSE";
        }

        /// <summary>
        /// 从名称末尾解析数字
        /// </summary>
        /// <param name="name">对象名称</param>
        /// <returns>末尾数字，不存在时返回 0</returns>
        private static int ParseTrailingNumber(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return 0;
            }

            int startIndex = name.Length;
            while (startIndex > 0 && char.IsDigit(name[startIndex - 1]))
            {
                --startIndex;
            }

            if (startIndex == name.Length)
            {
                return 0;
            }

            string numberText = name.Substring(startIndex);
            return int.TryParse(numberText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : 0;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Slot 配置导出标记的 Inspector 扩展
        /// </summary>
        [CustomEditor(typeof(SlotConfigAuthoring))]
        private sealed class SlotConfigAuthoringEditor : Editor
        {
            /// <summary>
            /// 绘制导出按钮
            /// </summary>
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                EditorGUILayout.Space();
                SlotConfigAuthoring authoring = (SlotConfigAuthoring)this.target;
                string resolvedPrefabPath = ResolvePrefabPath(authoring);

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("Luban 行预览", EditorStyles.boldLabel);
                    EditorGUILayout.SelectableLabel(GetPreview(authoring, resolvedPrefabPath), EditorStyles.textArea, GUILayout.MinHeight(36f));
                }

                if (GUILayout.Button("复制 Slot 配表行"))
                {
                    CopyLubanRow(authoring, resolvedPrefabPath);
                }

                if (GUILayout.Button("应用推断默认值"))
                {
                    ApplyInferredDefaults(authoring, resolvedPrefabPath);
                }
            }

            /// <summary>
            /// 复制当前 Slot 行到系统剪贴板
            /// </summary>
            /// <param name="authoring">目标导出标记</param>
            /// <param name="resolvedPrefabPath">已解析的 Slot 资源相对路径</param>
            private static void CopyLubanRow(SlotConfigAuthoring authoring, string resolvedPrefabPath)
            {
                string row = authoring.BuildLubanRow(resolvedPrefabPath);
                EditorGUIUtility.systemCopyBuffer = row;
                Debug.Log($"已复制 Slot 配表行: {row}", authoring);
            }

            /// <summary>
            /// 将推断出的 id 和资源路径写回当前组件
            /// </summary>
            /// <param name="authoring">目标导出标记</param>
            /// <param name="resolvedPrefabPath">已解析的 Slot 资源相对路径</param>
            private static void ApplyInferredDefaults(SlotConfigAuthoring authoring, string resolvedPrefabPath)
            {
                Undo.RecordObject(authoring, "应用 Slot 配置默认值");
                authoring.ApplyInferredDefaults(ParseTrailingNumber(authoring.gameObject.name), resolvedPrefabPath);
                EditorUtility.SetDirty(authoring);
            }

            /// <summary>
            /// 生成 Inspector 中的行预览
            /// </summary>
            /// <param name="authoring">目标导出标记</param>
            /// <param name="resolvedPrefabPath">已解析的 Slot 资源相对路径</param>
            /// <returns>预览文本</returns>
            private static string GetPreview(SlotConfigAuthoring authoring, string resolvedPrefabPath)
            {
                try
                {
                    return authoring.BuildLubanRow(resolvedPrefabPath);
                }
                catch (Exception exception)
                {
                    return exception.Message;
                }
            }

            /// <summary>
            /// 从 prefab 资源路径推断 Luban 中使用的资源相对路径
            /// </summary>
            /// <param name="authoring">目标导出标记</param>
            /// <returns>资源相对路径</returns>
            private static string ResolvePrefabPath(SlotConfigAuthoring authoring)
            {
                string prefabPathOverride = authoring.GetPrefabPathOverride();
                if (!string.IsNullOrWhiteSpace(prefabPathOverride))
                {
                    return NormalizeAssetPath(prefabPathOverride);
                }

                string assetPath = AssetDatabase.GetAssetPath(authoring.gameObject);
                if (string.IsNullOrWhiteSpace(assetPath))
                {
                    UnityEngine.Object prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(authoring.gameObject);
                    assetPath = AssetDatabase.GetAssetPath(prefabSource);
                }

                return NormalizeAssetPath(assetPath);
            }

            /// <summary>
            /// 将 Unity 资源路径转换为 Slot 配表中使用的相对路径
            /// </summary>
            /// <param name="assetPath">Unity 资源路径</param>
            /// <returns>配表资源相对路径</returns>
            private static string NormalizeAssetPath(string assetPath)
            {
                if (string.IsNullOrWhiteSpace(assetPath))
                {
                    return string.Empty;
                }

                string normalizedPath = assetPath.Replace('\\', '/');
                const string resourceRoot = "Packages/cn.etetet.gameplay/Resources/";
                if (normalizedPath.StartsWith(resourceRoot, StringComparison.Ordinal))
                {
                    normalizedPath = normalizedPath.Substring(resourceRoot.Length);
                }

                const string prefabExtension = ".prefab";
                if (normalizedPath.EndsWith(prefabExtension, StringComparison.OrdinalIgnoreCase))
                {
                    normalizedPath = normalizedPath.Substring(0, normalizedPath.Length - prefabExtension.Length);
                }

                return normalizedPath.TrimStart('/');
            }
        }
#endif
    }
}
