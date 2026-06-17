using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ET.Client
{
    /// <summary>
    /// Puzzle 预制体配置导出标记
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PuzzleConfigAuthoring : MonoBehaviour
    {
        [SerializeField]
        private int configId;

        [SerializeField]
        private string configName;

        [SerializeField]
        private string prefabPathOverride;

        [SerializeField]
        private float cellSize = 3f;

        [SerializeField]
        private bool includeLubanMarkerColumn = true;

        /// <summary>
        /// 根据当前字段生成可粘贴到 Puzzle.xlsx 的制表符行
        /// </summary>
        /// <param name="resolvedPrefabPath">已解析的 Puzzle 资源相对路径</param>
        /// <param name="resolvedSlotOffset">已解析的形状格偏移文本</param>
        /// <returns>可直接复制到 Luban 表的行文本</returns>
        public string BuildLubanRow(string resolvedPrefabPath, string resolvedSlotOffset)
        {
            this.ValidateConfig(resolvedPrefabPath, resolvedSlotOffset);

            string[] columns = includeLubanMarkerColumn
                    ? new[]
                    {
                            string.Empty,
                            FormatInt(configId),
                            NormalizeCell(configName),
                            NormalizeCell(resolvedPrefabPath),
                            NormalizeCell(resolvedSlotOffset),
                    }
                    : new[]
                    {
                            FormatInt(configId),
                            NormalizeCell(configName),
                            NormalizeCell(resolvedPrefabPath),
                            NormalizeCell(resolvedSlotOffset),
                    };

            return string.Join("\t", columns);
        }

        /// <summary>
        /// 获取当前 Puzzle 配置 id
        /// </summary>
        /// <returns>Puzzle 配置 id</returns>
        public int GetConfigId()
        {
            return configId;
        }

        /// <summary>
        /// 获取当前 Puzzle 配置名
        /// </summary>
        /// <returns>Puzzle 配置名</returns>
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
        /// 从子锚点自动生成形状格偏移文本
        /// </summary>
        /// <returns>形状格偏移文本</returns>
        public string BuildInferredSlotOffsetText()
        {
            List<Vector2Int> offsets = this.CollectSlotOffsets();
            ValidateSlotOffsets(offsets);
            return FormatSlotOffsets(offsets);
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
            cellSize = 3f;
            includeLubanMarkerColumn = true;
        }

        /// <summary>
        /// 从子 BoxCollider2D 锚点收集形状格偏移
        /// </summary>
        /// <returns>形状格偏移列表</returns>
        private List<Vector2Int> CollectSlotOffsets()
        {
            if (cellSize <= 0f)
            {
                throw new InvalidOperationException($"{nameof(PuzzleConfigAuthoring)} cell size is invalid: {cellSize}");
            }

            List<Vector2Int> offsets = new List<Vector2Int>();
            BoxCollider2D[] boxColliders = this.GetComponentsInChildren<BoxCollider2D>(true);
            foreach (BoxCollider2D boxCollider in boxColliders)
            {
                if (boxCollider == null || boxCollider.transform == this.transform)
                {
                    continue;
                }

                Vector3 localPosition = this.transform.InverseTransformPoint(boxCollider.transform.position);
                offsets.Add(ToSlotOffset(localPosition));
            }

            if (offsets.Count == 0)
            {
                offsets.Add(Vector2Int.zero);
            }

            offsets.Sort(CompareSlotOffset);
            return offsets;
        }

        /// <summary>
        /// 将本地坐标转换为离散形状格偏移
        /// </summary>
        /// <param name="localPosition">锚点相对 Puzzle 根节点的本地坐标</param>
        /// <returns>离散形状格偏移</returns>
        private Vector2Int ToSlotOffset(Vector3 localPosition)
        {
            float rawX = localPosition.x / cellSize;
            float rawY = -localPosition.y / cellSize;
            int offsetX = Mathf.RoundToInt(rawX);
            int offsetY = Mathf.RoundToInt(rawY);
            if (Mathf.Abs(rawX - offsetX) > 0.01f || Mathf.Abs(rawY - offsetY) > 0.01f)
            {
                throw new InvalidOperationException($"{nameof(PuzzleConfigAuthoring)} slot anchor is not aligned to grid: {localPosition}");
            }

            return new Vector2Int(offsetX, offsetY);
        }

        /// <summary>
        /// 校验当前字段是否足以生成 Puzzle 配置
        /// </summary>
        /// <param name="resolvedPrefabPath">已解析的 Puzzle 资源相对路径</param>
        /// <param name="resolvedSlotOffset">已解析的形状格偏移文本</param>
        private void ValidateConfig(string resolvedPrefabPath, string resolvedSlotOffset)
        {
            if (configId <= 0)
            {
                throw new InvalidOperationException($"{nameof(PuzzleConfigAuthoring)} config id is invalid: {configId}");
            }

            if (string.IsNullOrWhiteSpace(configName))
            {
                throw new InvalidOperationException($"{nameof(PuzzleConfigAuthoring)} config name is empty");
            }

            if (string.IsNullOrWhiteSpace(resolvedPrefabPath))
            {
                throw new InvalidOperationException($"{nameof(PuzzleConfigAuthoring)} prefab path is empty");
            }

            ValidateSlotOffsetText(resolvedSlotOffset);
        }

        /// <summary>
        /// 校验形状格偏移文本
        /// </summary>
        /// <param name="slotOffsetText">形状格偏移文本</param>
        private static void ValidateSlotOffsetText(string slotOffsetText)
        {
            if (string.IsNullOrWhiteSpace(slotOffsetText))
            {
                throw new InvalidOperationException($"{nameof(PuzzleConfigAuthoring)} slot offset is empty");
            }

            HashSet<Vector2Int> offsets = new HashSet<Vector2Int>();
            bool hasOrigin = false;
            string[] offsetTexts = slotOffsetText.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string offsetText in offsetTexts)
            {
                string[] values = offsetText.Split(',');
                if (values.Length != 2
                    || !int.TryParse(values[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int offsetX)
                    || !int.TryParse(values[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int offsetY))
                {
                    throw new InvalidOperationException($"{nameof(PuzzleConfigAuthoring)} slot offset is invalid: {slotOffsetText}");
                }

                Vector2Int offset = new Vector2Int(offsetX, offsetY);
                if (!offsets.Add(offset))
                {
                    throw new InvalidOperationException($"{nameof(PuzzleConfigAuthoring)} slot offset duplicated: {offsetX},{offsetY}");
                }

                hasOrigin |= offset == Vector2Int.zero;
            }

            if (!hasOrigin)
            {
                throw new InvalidOperationException($"{nameof(PuzzleConfigAuthoring)} slot offset must contain 0,0");
            }
        }

        /// <summary>
        /// 校验自动收集到的形状格偏移
        /// </summary>
        /// <param name="offsets">形状格偏移列表</param>
        private static void ValidateSlotOffsets(List<Vector2Int> offsets)
        {
            if (offsets == null || offsets.Count == 0)
            {
                throw new InvalidOperationException($"{nameof(PuzzleConfigAuthoring)} slot offset is empty");
            }

            HashSet<Vector2Int> uniqueOffsets = new HashSet<Vector2Int>();
            bool hasOrigin = false;
            foreach (Vector2Int offset in offsets)
            {
                if (!uniqueOffsets.Add(offset))
                {
                    throw new InvalidOperationException($"{nameof(PuzzleConfigAuthoring)} slot offset duplicated: {offset.x},{offset.y}");
                }

                hasOrigin |= offset == Vector2Int.zero;
            }

            if (!hasOrigin)
            {
                throw new InvalidOperationException($"{nameof(PuzzleConfigAuthoring)} slot offset must contain 0,0");
            }
        }

        /// <summary>
        /// 将形状格偏移列表格式化为 Luban 单元格文本
        /// </summary>
        /// <param name="offsets">形状格偏移列表</param>
        /// <returns>Luban 单元格文本</returns>
        private static string FormatSlotOffsets(List<Vector2Int> offsets)
        {
            List<string> values = new List<string>(offsets.Count);
            foreach (Vector2Int offset in offsets)
            {
                values.Add($"{FormatInt(offset.x)},{FormatInt(offset.y)}");
            }

            return string.Join(";", values);
        }

        /// <summary>
        /// 对形状格偏移排序，原点优先，其余格按距离和坐标稳定排序
        /// </summary>
        /// <param name="left">左侧偏移</param>
        /// <param name="right">右侧偏移</param>
        /// <returns>排序比较结果</returns>
        private static int CompareSlotOffset(Vector2Int left, Vector2Int right)
        {
            if (left == Vector2Int.zero)
            {
                return right == Vector2Int.zero ? 0 : -1;
            }

            if (right == Vector2Int.zero)
            {
                return 1;
            }

            int leftDistance = left.sqrMagnitude;
            int rightDistance = right.sqrMagnitude;
            if (leftDistance != rightDistance)
            {
                return leftDistance.CompareTo(rightDistance);
            }

            int yCompare = left.y.CompareTo(right.y);
            return yCompare != 0 ? yCompare : left.x.CompareTo(right.x);
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
        /// Puzzle 配置导出标记的 Inspector 扩展
        /// </summary>
        [CustomEditor(typeof(PuzzleConfigAuthoring))]
        private sealed class PuzzleConfigAuthoringEditor : Editor
        {
            /// <summary>
            /// 绘制导出按钮
            /// </summary>
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                EditorGUILayout.Space();
                PuzzleConfigAuthoring authoring = (PuzzleConfigAuthoring)this.target;
                string resolvedPrefabPath = ResolvePrefabPath(authoring);
                string resolvedSlotOffset = GetInferredSlotOffset(authoring);

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("Luban 行预览", EditorStyles.boldLabel);
                    EditorGUILayout.SelectableLabel(GetPreview(authoring, resolvedPrefabPath, resolvedSlotOffset), EditorStyles.textArea, GUILayout.MinHeight(36f));
                }

                if (GUILayout.Button("复制 Puzzle 配表行"))
                {
                    CopyLubanRow(authoring, resolvedPrefabPath, resolvedSlotOffset);
                }

                if (GUILayout.Button("应用推断默认值"))
                {
                    ApplyInferredDefaults(authoring, resolvedPrefabPath);
                }
            }

            /// <summary>
            /// 复制当前 Puzzle 行到系统剪贴板
            /// </summary>
            /// <param name="authoring">目标导出标记</param>
            /// <param name="resolvedPrefabPath">已解析的 Puzzle 资源相对路径</param>
            /// <param name="resolvedSlotOffset">已解析的形状格偏移文本</param>
            private static void CopyLubanRow(PuzzleConfigAuthoring authoring, string resolvedPrefabPath, string resolvedSlotOffset)
            {
                string row = authoring.BuildLubanRow(resolvedPrefabPath, resolvedSlotOffset);
                EditorGUIUtility.systemCopyBuffer = row;
                Debug.Log($"已复制 Puzzle 配表行: {row}", authoring);
            }

            /// <summary>
            /// 将推断出的 id 和资源路径写回当前组件
            /// </summary>
            /// <param name="authoring">目标导出标记</param>
            /// <param name="resolvedPrefabPath">已解析的 Puzzle 资源相对路径</param>
            private static void ApplyInferredDefaults(PuzzleConfigAuthoring authoring, string resolvedPrefabPath)
            {
                Undo.RecordObject(authoring, "应用 Puzzle 配置默认值");
                authoring.ApplyInferredDefaults(ParseTrailingNumber(authoring.gameObject.name), resolvedPrefabPath);
                EditorUtility.SetDirty(authoring);
            }

            /// <summary>
            /// 生成 Inspector 中的行预览
            /// </summary>
            /// <param name="authoring">目标导出标记</param>
            /// <param name="resolvedPrefabPath">已解析的 Puzzle 资源相对路径</param>
            /// <param name="resolvedSlotOffset">已解析的形状格偏移文本</param>
            /// <returns>预览文本</returns>
            private static string GetPreview(PuzzleConfigAuthoring authoring, string resolvedPrefabPath, string resolvedSlotOffset)
            {
                try
                {
                    return authoring.BuildLubanRow(resolvedPrefabPath, resolvedSlotOffset);
                }
                catch (Exception exception)
                {
                    return exception.Message;
                }
            }

            /// <summary>
            /// 从子锚点解析形状格偏移文本
            /// </summary>
            /// <param name="authoring">目标导出标记</param>
            /// <returns>形状格偏移文本</returns>
            private static string GetInferredSlotOffset(PuzzleConfigAuthoring authoring)
            {
                try
                {
                    return authoring.BuildInferredSlotOffsetText();
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
            private static string ResolvePrefabPath(PuzzleConfigAuthoring authoring)
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
            /// 将 Unity 资源路径转换为 Puzzle 配表中使用的相对路径
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
