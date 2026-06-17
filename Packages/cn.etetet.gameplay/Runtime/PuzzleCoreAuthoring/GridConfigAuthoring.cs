using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using UnityEditor;
#endif

namespace ET.Client
{
    /// <summary>
    /// Grid 预制体配置导出和反建标记
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GridConfigAuthoring : MonoBehaviour
    {
        [SerializeField]
        private int configId;

        [SerializeField]
        private string configName;

        [SerializeField]
        private int width = 3;

        [SerializeField]
        private int height = 3;

        [SerializeField]
        private float cellSize = 3f;

        [SerializeField]
        private string prefabPathOverride;

        [SerializeField]
        private string gridWorkbookPath = "Packages/cn.etetet.yiuilubangen/Luban/Config/Datas/Grid.xlsx";

        [SerializeField]
        private string slotWorkbookPath = "Packages/cn.etetet.yiuilubangen/Luban/Config/Datas/Slot.xlsx";

        [SerializeField]
        private string puzzleWorkbookPath = "Packages/cn.etetet.yiuilubangen/Luban/Config/Datas/Puzzle.xlsx";

        [SerializeField]
        private string slotRootName = "SlotTransform";

        [SerializeField]
        private string puzzleRootName = "PuzzleTransform";

        [SerializeField]
        private bool includeLubanMarkerColumn = true;

        /// <summary>
        /// 获取当前 Grid 配置 id
        /// </summary>
        /// <returns>Grid 配置 id</returns>
        public int GetConfigId()
        {
            return configId;
        }

        /// <summary>
        /// 获取当前 Grid 配置名
        /// </summary>
        /// <returns>Grid 配置名</returns>
        public string GetConfigName()
        {
            return configName;
        }

        /// <summary>
        /// 获取当前 Grid 宽度
        /// </summary>
        /// <returns>Grid 宽度</returns>
        public int GetWidth()
        {
            return width;
        }

        /// <summary>
        /// 获取当前 Grid 高度
        /// </summary>
        /// <returns>Grid 高度</returns>
        public int GetHeight()
        {
            return height;
        }

        /// <summary>
        /// 获取格子间距
        /// </summary>
        /// <returns>格子间距</returns>
        public float GetCellSize()
        {
            return cellSize;
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
        /// 获取 Grid 配置表路径
        /// </summary>
        /// <returns>Grid 配置表路径</returns>
        public string GetGridWorkbookPath()
        {
            return gridWorkbookPath;
        }

        /// <summary>
        /// 获取 Slot 配置表路径
        /// </summary>
        /// <returns>Slot 配置表路径</returns>
        public string GetSlotWorkbookPath()
        {
            return slotWorkbookPath;
        }

        /// <summary>
        /// 获取 Puzzle 配置表路径
        /// </summary>
        /// <returns>Puzzle 配置表路径</returns>
        public string GetPuzzleWorkbookPath()
        {
            return puzzleWorkbookPath;
        }

        /// <summary>
        /// 获取 Slot 挂载根节点名称
        /// </summary>
        /// <returns>Slot 挂载根节点名称</returns>
        public string GetSlotRootName()
        {
            return slotRootName;
        }

        /// <summary>
        /// 获取 Puzzle 挂载根节点名称
        /// </summary>
        /// <returns>Puzzle 挂载根节点名称</returns>
        public string GetPuzzleRootName()
        {
            return puzzleRootName;
        }

        /// <summary>
        /// 当前复制文本是否包含 Luban 标记空列
        /// </summary>
        /// <returns>是否包含 Luban 标记空列</returns>
        public bool ShouldIncludeLubanMarkerColumn()
        {
            return includeLubanMarkerColumn;
        }

        /// <summary>
        /// 用表格数据写回当前 authoring 字段
        /// </summary>
        /// <param name="gridName">Grid 名称</param>
        /// <param name="gridWidth">Grid 宽度</param>
        /// <param name="gridHeight">Grid 高度</param>
        /// <param name="gridPrefabPath">Grid 资源路径</param>
        public void ApplyGridConfigData(string gridName, int gridWidth, int gridHeight, string gridPrefabPath)
        {
            configName = gridName;
            width = gridWidth;
            height = gridHeight;
            prefabPathOverride = gridPrefabPath;
        }

        /// <summary>
        /// 编辑器重置时按对象名补齐易推断字段
        /// </summary>
        private void Reset()
        {
            configId = ParseTrailingNumber(this.gameObject.name);
            configName = this.gameObject.name;
            width = 3;
            height = 3;
            cellSize = 3f;
            prefabPathOverride = $"Grid/{this.gameObject.name}";
            gridWorkbookPath = "Packages/cn.etetet.yiuilubangen/Luban/Config/Datas/Grid.xlsx";
            slotWorkbookPath = "Packages/cn.etetet.yiuilubangen/Luban/Config/Datas/Slot.xlsx";
            puzzleWorkbookPath = "Packages/cn.etetet.yiuilubangen/Luban/Config/Datas/Puzzle.xlsx";
            slotRootName = "SlotTransform";
            puzzleRootName = "PuzzleTransform";
            includeLubanMarkerColumn = true;
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
        /// Grid 配置导出和反建的 Inspector 扩展
        /// </summary>
        [CustomEditor(typeof(GridConfigAuthoring))]
        private sealed class GridConfigAuthoringEditor : Editor
        {
            private const float CoordinateTolerance = 0.01f;

            /// <summary>
            /// 绘制 Grid 配置工具按钮
            /// </summary>
            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                DrawGridFields();
                serializedObject.ApplyModifiedProperties();

                EditorGUILayout.Space();
                GridConfigAuthoring authoring = (GridConfigAuthoring)this.target;
                if (GUILayout.Button("从 Grid.xlsx 生成关卡"))
                {
                    RunEditorAction(authoring, () => GenerateLevelFromWorkbook(authoring));
                }

                if (GUILayout.Button("校验当前 Grid"))
                {
                    RunEditorAction(authoring, () => ValidateCurrentGrid(authoring));
                }

                if (GUILayout.Button("复制 Grid 配表块"))
                {
                    RunEditorAction(authoring, () => CopyGridBlock(authoring));
                }

                if (GUILayout.Button("清理 Slot 和 Puzzle"))
                {
                    RunEditorAction(authoring, () => ClearLevelObjects(authoring));
                }
            }

            /// <summary>
            /// 绘制 Grid 配置字段
            /// </summary>
            private void DrawGridFields()
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("configId"), new GUIContent("配置 Id"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("configName"), new GUIContent("配置名"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("width"), new GUIContent("宽度"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("height"), new GUIContent("高度"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("cellSize"), new GUIContent("格子间距"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("prefabPathOverride"), new GUIContent("Prefab 路径覆盖"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("gridWorkbookPath"), new GUIContent("Grid 表路径"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("slotWorkbookPath"), new GUIContent("Slot 表路径"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("puzzleWorkbookPath"), new GUIContent("Puzzle 表路径"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("slotRootName"), new GUIContent("Slot 根节点名"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("puzzleRootName"), new GUIContent("Puzzle 根节点名"));
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty("includeLubanMarkerColumn"),
                    new GUIContent("包含 Luban 标记列", "开启后复制内容第一列为空，可从表格 A 列粘贴；关闭后从 Id 列粘贴"));
            }

            /// <summary>
            /// 执行编辑器操作并统一处理异常提示
            /// </summary>
            /// <param name="authoring">目标 Grid 标记</param>
            /// <param name="action">编辑器操作</param>
            private static void RunEditorAction(GridConfigAuthoring authoring, Action action)
            {
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    Debug.LogError(exception.Message, authoring);
                }
            }

            /// <summary>
            /// 从 Grid.xlsx、Slot.xlsx 和 Puzzle.xlsx 反建当前关卡
            /// </summary>
            /// <param name="authoring">目标 Grid 标记</param>
            private static void GenerateLevelFromWorkbook(GridConfigAuthoring authoring)
            {
                GridAuthoringTables tables = LoadTables(authoring);
                GridConfigRecord gridConfig = tables.GetGrid(authoring.GetConfigId());
                Undo.RecordObject(authoring, "从 Grid 配表应用配置");
                authoring.ApplyGridConfigData(gridConfig.Name, gridConfig.Width, gridConfig.Height, gridConfig.PrefabPath);
                EditorUtility.SetDirty(authoring);

                Transform slotRoot = FindRequiredChild(authoring.transform, authoring.GetSlotRootName());
                Transform puzzleRoot = FindRequiredChild(authoring.transform, authoring.GetPuzzleRootName());
                ClearChildren(slotRoot);
                ClearChildren(puzzleRoot);
                ApplySlotRootLayout(slotRoot, gridConfig.Width, authoring.GetCellSize());

                for (int y = 0; y < gridConfig.Height; ++y)
                {
                    for (int x = 0; x < gridConfig.Width; ++x)
                    {
                        int slotIndex = y * gridConfig.Width + x;
                        SlotConfigRecord slotConfig = tables.GetSlot(gridConfig.SlotList[slotIndex]);
                        GameObject slotPrefab = LoadPrefab(slotConfig.PrefabPath);
                        GameObject slotInstance = InstantiatePrefab(slotPrefab, slotRoot, $"Slot_{x}_{y}_{slotConfig.Id}");
                        slotInstance.transform.localPosition = GetGridLocalPosition(gridConfig.Width, gridConfig.Height, authoring.GetCellSize(), x, y);
                    }
                }

                for (int index = 0; index < gridConfig.Puzzles.Count; ++index)
                {
                    GridPuzzleRecord puzzleInfo = gridConfig.Puzzles[index];
                    PuzzleConfigRecord puzzleConfig = tables.GetPuzzle(puzzleInfo.Id);
                    GameObject puzzlePrefab = LoadPrefab(puzzleConfig.PrefabPath);
                    GameObject puzzleInstance = InstantiatePrefab(puzzlePrefab, puzzleRoot, $"Puzzle_{index + 1}_{puzzleInfo.Id}");
                    puzzleInstance.transform.localPosition = puzzleInfo.LocalPosition;
                }

                EditorUtility.SetDirty(authoring.gameObject);
                Debug.Log($"已从 Grid.xlsx 生成关卡: id={gridConfig.Id} slot={gridConfig.SlotList.Count} puzzle={gridConfig.Puzzles.Count}", authoring);
            }

            /// <summary>
            /// 校验当前 Grid 下的 Slot 和 Puzzle 是否能生成配表块
            /// </summary>
            /// <param name="authoring">目标 Grid 标记</param>
            private static void ValidateCurrentGrid(GridConfigAuthoring authoring)
            {
                GridExportData exportData = CollectCurrentGrid(authoring);
                Debug.Log($"Grid 校验通过: id={exportData.Id} slot={exportData.SlotList.Count} puzzle={exportData.Puzzles.Count}", authoring);
            }

            /// <summary>
            /// 复制当前 Grid 的多行配表块
            /// </summary>
            /// <param name="authoring">目标 Grid 标记</param>
            private static void CopyGridBlock(GridConfigAuthoring authoring)
            {
                GridExportData exportData = CollectCurrentGrid(authoring);
                string block = BuildGridLubanBlock(authoring, exportData);
                EditorGUIUtility.systemCopyBuffer = block;
                Debug.Log($"已复制 Grid 配表块:\n{block}", authoring);
            }

            /// <summary>
            /// 清理 SlotTransform 和 PuzzleTransform 下的全部子对象
            /// </summary>
            /// <param name="authoring">目标 Grid 标记</param>
            private static void ClearLevelObjects(GridConfigAuthoring authoring)
            {
                Transform slotRoot = FindRequiredChild(authoring.transform, authoring.GetSlotRootName());
                Transform puzzleRoot = FindRequiredChild(authoring.transform, authoring.GetPuzzleRootName());
                ClearChildren(slotRoot);
                ClearChildren(puzzleRoot);
                EditorUtility.SetDirty(authoring.gameObject);
                Debug.Log("已清理 Slot 和 Puzzle", authoring);
            }

            /// <summary>
            /// 收集当前 Grid 下的 Slot 和 Puzzle 数据
            /// </summary>
            /// <param name="authoring">目标 Grid 标记</param>
            /// <returns>Grid 导出数据</returns>
            private static GridExportData CollectCurrentGrid(GridConfigAuthoring authoring)
            {
                ValidateGridAuthoring(authoring);
                Transform slotRoot = FindRequiredChild(authoring.transform, authoring.GetSlotRootName());
                Transform puzzleRoot = FindRequiredChild(authoring.transform, authoring.GetPuzzleRootName());
                int width = authoring.GetWidth();
                int height = authoring.GetHeight();
                int slotCount = width * height;
                List<int> slotList = Enumerable.Repeat(0, slotCount).ToList();
                bool[] occupiedSlots = new bool[slotCount];

                for (int i = 0; i < slotRoot.childCount; ++i)
                {
                    Transform child = slotRoot.GetChild(i);
                    SlotConfigAuthoring slotAuthoring = child.GetComponent<SlotConfigAuthoring>();
                    if (slotAuthoring == null)
                    {
                        throw new InvalidOperationException($"Slot 子节点缺少 SlotConfigAuthoring: {child.name}");
                    }

                    Vector2Int coordinate = ToGridCoordinate(authoring, child.localPosition);
                    if (coordinate.x < 0 || coordinate.x >= width || coordinate.y < 0 || coordinate.y >= height)
                    {
                        throw new InvalidOperationException($"Slot 坐标越界: {child.name} ({coordinate.x},{coordinate.y})");
                    }

                    int slotIndex = coordinate.y * width + coordinate.x;
                    if (occupiedSlots[slotIndex])
                    {
                        throw new InvalidOperationException($"Slot 坐标重复: ({coordinate.x},{coordinate.y})");
                    }

                    occupiedSlots[slotIndex] = true;
                    slotList[slotIndex] = slotAuthoring.GetConfigId();
                }

                for (int i = 0; i < occupiedSlots.Length; ++i)
                {
                    if (!occupiedSlots[i])
                    {
                        int x = i % width;
                        int y = i / width;
                        throw new InvalidOperationException($"缺少 Slot: ({x},{y})");
                    }
                }

                List<GridPuzzleExportData> puzzles = new List<GridPuzzleExportData>();
                for (int i = 0; i < puzzleRoot.childCount; ++i)
                {
                    Transform child = puzzleRoot.GetChild(i);
                    PuzzleConfigAuthoring puzzleAuthoring = child.GetComponent<PuzzleConfigAuthoring>();
                    if (puzzleAuthoring == null)
                    {
                        throw new InvalidOperationException($"Puzzle 子节点缺少 PuzzleConfigAuthoring: {child.name}");
                    }

                    puzzles.Add(new GridPuzzleExportData()
                    {
                            Id = puzzleAuthoring.GetConfigId(),
                            LocalPosition = child.localPosition,
                    });
                }

                return new GridExportData()
                {
                        Id = authoring.GetConfigId(),
                        Name = authoring.GetConfigName(),
                        Width = width,
                        Height = height,
                        PrefabPath = ResolveGridPrefabPath(authoring),
                        SlotList = slotList,
                        Puzzles = puzzles,
                };
            }

            /// <summary>
            /// 生成 Grid.xlsx 可粘贴的多行文本
            /// </summary>
            /// <param name="authoring">目标 Grid 标记</param>
            /// <param name="exportData">Grid 导出数据</param>
            /// <returns>多行配表文本</returns>
            private static string BuildGridLubanBlock(GridConfigAuthoring authoring, GridExportData exportData)
            {
                List<string> rows = new List<string>();
                int puzzleRowCount = Math.Max(1, exportData.Puzzles.Count);
                string slotListText = string.Join(";", exportData.SlotList.Select(FormatInt));
                for (int rowIndex = 0; rowIndex < puzzleRowCount; ++rowIndex)
                {
                    GridPuzzleExportData puzzle = rowIndex < exportData.Puzzles.Count ? exportData.Puzzles[rowIndex] : null;
                    rows.Add(string.Join("\t", BuildGridRowColumns(authoring.ShouldIncludeLubanMarkerColumn(), exportData, puzzle, rowIndex == 0, slotListText)));
                }

                return string.Join("\n", rows);
            }

            /// <summary>
            /// 构建 Grid 配表中的单行列值
            /// </summary>
            /// <param name="includeMarkerColumn">是否包含 Luban 标记空列</param>
            /// <param name="exportData">Grid 导出数据</param>
            /// <param name="puzzle">当前行 Puzzle 数据</param>
            /// <param name="isFirstRow">是否为当前 Grid 的首行</param>
            /// <param name="slotListText">完整 SlotList 文本</param>
            /// <returns>当前行列值</returns>
            private static string[] BuildGridRowColumns(bool includeMarkerColumn, GridExportData exportData, GridPuzzleExportData puzzle, bool isFirstRow, string slotListText)
            {
                List<string> columns = new List<string>();
                if (includeMarkerColumn)
                {
                    columns.Add(string.Empty);
                }

                if (isFirstRow)
                {
                    columns.Add(FormatInt(exportData.Id));
                    columns.Add(NormalizeCell(exportData.Name));
                    columns.Add(FormatInt(exportData.Width));
                    columns.Add(FormatInt(exportData.Height));
                    columns.Add(FormatInt(exportData.Puzzles.Count));
                    columns.Add(NormalizeCell(exportData.PrefabPath));
                }
                else
                {
                    columns.AddRange(new[] { string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty });
                }

                if (puzzle != null)
                {
                    columns.Add(FormatInt(puzzle.Id));
                    columns.Add(FormatFloat(puzzle.LocalPosition.x));
                    columns.Add(FormatFloat(puzzle.LocalPosition.y));
                    columns.Add(FormatFloat(puzzle.LocalPosition.z));
                }
                else
                {
                    columns.AddRange(new[] { string.Empty, string.Empty, string.Empty, string.Empty });
                }

                columns.Add(isFirstRow ? slotListText : string.Empty);
                return columns.ToArray();
            }

            /// <summary>
            /// 校验 Grid 标记基础字段
            /// </summary>
            /// <param name="authoring">目标 Grid 标记</param>
            private static void ValidateGridAuthoring(GridConfigAuthoring authoring)
            {
                if (authoring.GetConfigId() <= 0)
                {
                    throw new InvalidOperationException($"Grid 配置 id 无效: {authoring.GetConfigId()}");
                }

                if (string.IsNullOrWhiteSpace(authoring.GetConfigName()))
                {
                    throw new InvalidOperationException("Grid 配置名为空");
                }

                if (authoring.GetWidth() <= 0 || authoring.GetHeight() <= 0)
                {
                    throw new InvalidOperationException($"Grid 尺寸无效: {authoring.GetWidth()}x{authoring.GetHeight()}");
                }

                if (authoring.GetCellSize() <= 0f)
                {
                    throw new InvalidOperationException($"Grid cellSize 无效: {authoring.GetCellSize()}");
                }
            }

            /// <summary>
            /// 将 Slot 本地坐标转换为 Grid 坐标
            /// </summary>
            /// <param name="authoring">目标 Grid 标记</param>
            /// <param name="localPosition">Slot 本地坐标</param>
            /// <returns>Grid 坐标</returns>
            private static Vector2Int ToGridCoordinate(GridConfigAuthoring authoring, Vector3 localPosition)
            {
                float rawX = localPosition.x / authoring.GetCellSize() + (authoring.GetWidth() - 1) * 0.5f;
                float rawY = (authoring.GetHeight() - 1) * 0.5f - localPosition.y / authoring.GetCellSize();
                int x = Mathf.RoundToInt(rawX);
                int y = Mathf.RoundToInt(rawY);
                if (Mathf.Abs(rawX - x) > CoordinateTolerance || Mathf.Abs(rawY - y) > CoordinateTolerance)
                {
                    throw new InvalidOperationException(
                        $"Slot 未匹配当前 Grid 格点: local={FormatVector3(localPosition)}, raw=({FormatFloat(rawX)},{FormatFloat(rawY)}), grid={authoring.GetWidth()}x{authoring.GetHeight()}, cellSize={FormatFloat(authoring.GetCellSize())}");
                }

                return new Vector2Int(x, y);
            }

            /// <summary>
            /// 获取指定 Grid 坐标对应的本地坐标
            /// </summary>
            /// <param name="width">Grid 宽度</param>
            /// <param name="height">Grid 高度</param>
            /// <param name="cellSize">格子间距</param>
            /// <param name="x">Grid X 坐标</param>
            /// <param name="y">Grid Y 坐标</param>
            /// <returns>本地坐标</returns>
            private static Vector3 GetGridLocalPosition(int width, int height, float cellSize, int x, int y)
            {
                float localX = (x - (width - 1) * 0.5f) * cellSize;
                float localY = ((height - 1) * 0.5f - y) * cellSize;
                return new Vector3(localX, localY, 0f);
            }

            /// <summary>
            /// 加载并缓存三张配置表
            /// </summary>
            /// <param name="authoring">目标 Grid 标记</param>
            /// <returns>配置表数据</returns>
            private static GridAuthoringTables LoadTables(GridConfigAuthoring authoring)
            {
                string gridPath = ResolveProjectPath(authoring.GetGridWorkbookPath());
                string slotPath = ResolveProjectPath(authoring.GetSlotWorkbookPath());
                string puzzlePath = ResolveProjectPath(authoring.GetPuzzleWorkbookPath());
                return GridAuthoringXlsxReader.Load(gridPath, slotPath, puzzlePath);
            }

            /// <summary>
            /// 查找必需子节点
            /// </summary>
            /// <param name="parent">父节点</param>
            /// <param name="childName">子节点名称</param>
            /// <returns>找到的子节点</returns>
            private static Transform FindRequiredChild(Transform parent, string childName)
            {
                Transform child = parent.Find(childName);
                if (child == null)
                {
                    throw new InvalidOperationException($"缺少节点: {childName}");
                }

                return child;
            }

            /// <summary>
            /// 清理指定节点下全部子对象
            /// </summary>
            /// <param name="root">目标根节点</param>
            private static void ClearChildren(Transform root)
            {
                for (int i = root.childCount - 1; i >= 0; --i)
                {
                    Undo.DestroyObjectImmediate(root.GetChild(i).gameObject);
                }
            }

            /// <summary>
            /// 加载资源路径对应的 prefab
            /// </summary>
            /// <param name="prefabPath">配表中的资源路径</param>
            /// <returns>加载到的 prefab</returns>
            private static GameObject LoadPrefab(string prefabPath)
            {
                string assetPath = ToPrefabAssetPath(prefabPath);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab == null)
                {
                    throw new InvalidOperationException($"prefab 不存在: {assetPath}");
                }

                return prefab;
            }

            /// <summary>
            /// 实例化 prefab 并设置父节点和名称
            /// </summary>
            /// <param name="prefab">目标 prefab</param>
            /// <param name="parent">挂载父节点</param>
            /// <param name="instanceName">实例名称</param>
            /// <returns>实例对象</returns>
            private static GameObject InstantiatePrefab(GameObject prefab, Transform parent, string instanceName)
            {
                GameObject instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
                if (instance == null)
                {
                    throw new InvalidOperationException($"prefab 实例化失败: {prefab.name}");
                }

                Undo.RegisterCreatedObjectUndo(instance, "生成 Grid 关卡对象");
                instance.name = instanceName;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
                return instance;
            }

            /// <summary>
            /// 同步 Slot 根节点的居中网格布局参数
            /// </summary>
            /// <param name="slotRoot">Slot 根节点</param>
            /// <param name="width">Grid 宽度</param>
            /// <param name="cellSize">格子间距</param>
            private static void ApplySlotRootLayout(Transform slotRoot, int width, float cellSize)
            {
                MonoBehaviour layout = FindCenteredGridLayout(slotRoot);
                if (layout == null)
                {
                    return;
                }

                Undo.RecordObject(layout, "同步 Slot 网格布局");
                SerializedObject serializedLayout = new SerializedObject(layout);
                SerializedProperty constraint = FindRequiredProperty(serializedLayout, "constraint");
                SerializedProperty constraintCount = FindRequiredProperty(serializedLayout, "constraintCount");
                SerializedProperty layoutCellSize = FindRequiredProperty(serializedLayout, "cellSize");
                SerializedProperty spacing = FindRequiredProperty(serializedLayout, "spacing");

                constraint.enumValueIndex = 1;
                constraintCount.intValue = width;
                layoutCellSize.vector2Value = new Vector2(cellSize, cellSize);
                spacing.vector2Value = Vector2.zero;
                serializedLayout.ApplyModifiedProperties();
                EditorUtility.SetDirty(layout);

                MethodInfo updateLayout = layout.GetType().GetMethod("UpdateLayout", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                updateLayout?.Invoke(layout, null);
            }

            /// <summary>
            /// 查找 Slot 根节点上的居中网格布局组件
            /// </summary>
            /// <param name="slotRoot">Slot 根节点</param>
            /// <returns>居中网格布局组件</returns>
            private static MonoBehaviour FindCenteredGridLayout(Transform slotRoot)
            {
                MonoBehaviour[] behaviours = slotRoot.GetComponents<MonoBehaviour>();
                for (int i = 0; i < behaviours.Length; ++i)
                {
                    MonoBehaviour behaviour = behaviours[i];
                    if (behaviour != null && behaviour.GetType().Name == "CenteredGridLayout")
                    {
                        return behaviour;
                    }
                }

                return null;
            }

            /// <summary>
            /// 查找必需序列化字段
            /// </summary>
            /// <param name="serializedObject">序列化对象</param>
            /// <param name="propertyName">字段名</param>
            /// <returns>序列化字段</returns>
            private static SerializedProperty FindRequiredProperty(SerializedObject serializedObject, string propertyName)
            {
                SerializedProperty property = serializedObject.FindProperty(propertyName);
                if (property == null)
                {
                    throw new InvalidOperationException($"CenteredGridLayout 缺少字段: {propertyName}");
                }

                return property;
            }

            /// <summary>
            /// 解析当前 Grid prefab 的配置路径
            /// </summary>
            /// <param name="authoring">目标 Grid 标记</param>
            /// <returns>Grid 配置路径</returns>
            private static string ResolveGridPrefabPath(GridConfigAuthoring authoring)
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
            /// 将配置路径转换为 Unity prefab 资源路径
            /// </summary>
            /// <param name="resourcePath">配置中的资源路径</param>
            /// <returns>Unity prefab 资源路径</returns>
            private static string ToPrefabAssetPath(string resourcePath)
            {
                string normalizedPath = NormalizeAssetPath(resourcePath);
                if (normalizedPath.StartsWith("Packages/", StringComparison.Ordinal))
                {
                    return normalizedPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase) ? normalizedPath : $"{normalizedPath}.prefab";
                }

                return $"Packages/cn.etetet.gameplay/Resources/{normalizedPath}.prefab";
            }

            /// <summary>
            /// 将 Unity 资源路径转换为配置中使用的相对路径
            /// </summary>
            /// <param name="assetPath">Unity 资源路径</param>
            /// <returns>配置资源相对路径</returns>
            private static string NormalizeAssetPath(string assetPath)
            {
                if (string.IsNullOrWhiteSpace(assetPath))
                {
                    return string.Empty;
                }

                string normalizedPath = assetPath.Replace('\\', '/').Trim();
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

            /// <summary>
            /// 将相对项目路径解析为绝对路径
            /// </summary>
            /// <param name="path">相对或绝对路径</param>
            /// <returns>绝对路径</returns>
            private static string ResolveProjectPath(string path)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    throw new InvalidOperationException("配置表路径为空");
                }

                string normalizedPath = path.Replace('\\', '/');
                if (Path.IsPathRooted(normalizedPath))
                {
                    return normalizedPath;
                }

                string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
                if (string.IsNullOrWhiteSpace(projectRoot))
                {
                    throw new InvalidOperationException("无法解析 Unity 项目根目录");
                }

                return Path.Combine(projectRoot, normalizedPath);
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
            /// 按无区域格式输出浮点数
            /// </summary>
            /// <param name="value">浮点数</param>
            /// <returns>浮点数文本</returns>
            private static string FormatFloat(float value)
            {
                return value.ToString("0.###", CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// 按无区域格式输出三维坐标
            /// </summary>
            /// <param name="value">三维坐标</param>
            /// <returns>三维坐标文本</returns>
            private static string FormatVector3(Vector3 value)
            {
                return $"({FormatFloat(value.x)},{FormatFloat(value.y)},{FormatFloat(value.z)})";
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
        }

        /// <summary>
        /// Grid authoring 使用的 xlsx 读取器
        /// </summary>
        private static class GridAuthoringXlsxReader
        {
            /// <summary>
            /// 读取三张配置表
            /// </summary>
            /// <param name="gridPath">Grid.xlsx 绝对路径</param>
            /// <param name="slotPath">Slot.xlsx 绝对路径</param>
            /// <param name="puzzlePath">Puzzle.xlsx 绝对路径</param>
            /// <returns>配置表数据</returns>
            public static GridAuthoringTables Load(string gridPath, string slotPath, string puzzlePath)
            {
                return new GridAuthoringTables()
                {
                        Slots = ReadSlotTable(slotPath),
                        Puzzles = ReadPuzzleTable(puzzlePath),
                        Grids = ReadGridTable(gridPath),
                };
            }

            /// <summary>
            /// 读取 Slot 配置表
            /// </summary>
            /// <param name="path">Slot.xlsx 路径</param>
            /// <returns>Slot 配置映射</returns>
            private static Dictionary<int, SlotConfigRecord> ReadSlotTable(string path)
            {
                XlsxSheet sheet = XlsxSheet.Load(path);
                int headerRow = sheet.FindRow("##var");
                int idColumn = sheet.FindColumn(headerRow, "Id");
                int nameColumn = sheet.FindColumn(headerRow, "Name");
                int prefabPathColumn = sheet.FindColumn(headerRow, "PrefabPath");
                Dictionary<int, SlotConfigRecord> result = new Dictionary<int, SlotConfigRecord>();
                for (int row = headerRow + 4; row < sheet.RowCount; ++row)
                {
                    string idText = sheet.GetCell(row, idColumn);
                    if (!TryParseInt(idText, out int id))
                    {
                        continue;
                    }

                    result.Add(id, new SlotConfigRecord()
                    {
                            Id = id,
                            Name = sheet.GetCell(row, nameColumn),
                            PrefabPath = sheet.GetCell(row, prefabPathColumn),
                    });
                }

                return result;
            }

            /// <summary>
            /// 读取 Puzzle 配置表
            /// </summary>
            /// <param name="path">Puzzle.xlsx 路径</param>
            /// <returns>Puzzle 配置映射</returns>
            private static Dictionary<int, PuzzleConfigRecord> ReadPuzzleTable(string path)
            {
                XlsxSheet sheet = XlsxSheet.Load(path);
                int headerRow = sheet.FindRow("##var");
                int idColumn = sheet.FindColumn(headerRow, "Id");
                int nameColumn = sheet.FindColumn(headerRow, "Name");
                int prefabPathColumn = sheet.FindColumn(headerRow, "PrefabPath");
                Dictionary<int, PuzzleConfigRecord> result = new Dictionary<int, PuzzleConfigRecord>();
                for (int row = headerRow + 4; row < sheet.RowCount; ++row)
                {
                    string idText = sheet.GetCell(row, idColumn);
                    if (!TryParseInt(idText, out int id))
                    {
                        continue;
                    }

                    result.Add(id, new PuzzleConfigRecord()
                    {
                            Id = id,
                            Name = sheet.GetCell(row, nameColumn),
                            PrefabPath = sheet.GetCell(row, prefabPathColumn),
                    });
                }

                return result;
            }

            /// <summary>
            /// 读取 Grid 配置表
            /// </summary>
            /// <param name="path">Grid.xlsx 路径</param>
            /// <returns>Grid 配置映射</returns>
            private static Dictionary<int, GridConfigRecord> ReadGridTable(string path)
            {
                XlsxSheet sheet = XlsxSheet.Load(path);
                int headerRow = sheet.FindRow("##var");
                int idColumn = sheet.FindColumn(headerRow, "Id");
                int nameColumn = sheet.FindColumn(headerRow, "Name");
                int widthColumn = sheet.FindColumn(headerRow, "X");
                int heightColumn = sheet.FindColumn(headerRow, "Y");
                int prefabPathColumn = sheet.FindColumn(headerRow, "PrefabPath");
                int puzzleStartColumn = sheet.FindOptionalColumn(headerRow, "*PuzzleList");
                if (puzzleStartColumn < 0)
                {
                    puzzleStartColumn = sheet.FindColumn(headerRow, "PuzzleList");
                }

                int slotListColumn = sheet.FindColumn(headerRow, "SlotList");
                Dictionary<int, GridConfigRecord> result = new Dictionary<int, GridConfigRecord>();
                for (int row = headerRow + 4; row < sheet.RowCount; ++row)
                {
                    string idText = sheet.GetCell(row, idColumn);
                    if (!TryParseInt(idText, out int id))
                    {
                        continue;
                    }

                    int blockEndRow = FindGridBlockEnd(sheet, row + 1, idColumn);
                    GridConfigRecord record = new GridConfigRecord()
                    {
                            Id = id,
                            Name = sheet.GetCell(row, nameColumn),
                            Width = ParseRequiredInt(sheet.GetCell(row, widthColumn), $"Grid {id} X"),
                            Height = ParseRequiredInt(sheet.GetCell(row, heightColumn), $"Grid {id} Y"),
                            PrefabPath = sheet.GetCell(row, prefabPathColumn),
                            SlotList = ParseIntList(sheet.GetCell(row, slotListColumn), $"Grid {id} SlotList"),
                            Puzzles = ReadGridPuzzles(sheet, row, blockEndRow, puzzleStartColumn),
                    };

                    result.Add(id, record);
                    row = blockEndRow - 1;
                }

                return result;
            }

            /// <summary>
            /// 查找 Grid 多行块结束行
            /// </summary>
            /// <param name="sheet">工作表</param>
            /// <param name="startRow">起始扫描行</param>
            /// <param name="idColumn">Id 列</param>
            /// <returns>结束行后一行</returns>
            private static int FindGridBlockEnd(XlsxSheet sheet, int startRow, int idColumn)
            {
                for (int row = startRow; row < sheet.RowCount; ++row)
                {
                    if (!string.IsNullOrWhiteSpace(sheet.GetCell(row, idColumn)))
                    {
                        return row;
                    }
                }

                return sheet.RowCount;
            }

            /// <summary>
            /// 读取 Grid 行块中的 PuzzleList
            /// </summary>
            /// <param name="sheet">工作表</param>
            /// <param name="startRow">起始行</param>
            /// <param name="endRow">结束行后一行</param>
            /// <param name="puzzleStartColumn">PuzzleList 起始列</param>
            /// <returns>Puzzle 实例列表</returns>
            private static List<GridPuzzleRecord> ReadGridPuzzles(XlsxSheet sheet, int startRow, int endRow, int puzzleStartColumn)
            {
                List<GridPuzzleRecord> puzzles = new List<GridPuzzleRecord>();
                for (int row = startRow; row < endRow; ++row)
                {
                    string puzzleIdText = sheet.GetCell(row, puzzleStartColumn);
                    if (!TryParseInt(puzzleIdText, out int puzzleId))
                    {
                        continue;
                    }

                    puzzles.Add(new GridPuzzleRecord()
                    {
                            Id = puzzleId,
                            LocalPosition = new Vector3(
                                    ParseOptionalFloat(sheet.GetCell(row, puzzleStartColumn + 1)),
                                    ParseOptionalFloat(sheet.GetCell(row, puzzleStartColumn + 2)),
                                    ParseOptionalFloat(sheet.GetCell(row, puzzleStartColumn + 3))),
                    });
                }

                return puzzles;
            }

            /// <summary>
            /// 解析整数列表
            /// </summary>
            /// <param name="text">列表文本</param>
            /// <param name="fieldName">字段名</param>
            /// <returns>整数列表</returns>
            private static List<int> ParseIntList(string text, string fieldName)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    throw new InvalidOperationException($"{fieldName} 为空");
                }

                List<int> result = new List<int>();
                string[] parts = text.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string part in parts)
                {
                    result.Add(ParseRequiredInt(part, fieldName));
                }

                return result;
            }

            /// <summary>
            /// 解析必需整数
            /// </summary>
            /// <param name="text">整数文本</param>
            /// <param name="fieldName">字段名</param>
            /// <returns>整数</returns>
            private static int ParseRequiredInt(string text, string fieldName)
            {
                if (!TryParseInt(text, out int value))
                {
                    throw new InvalidOperationException($"{fieldName} 不是有效整数: {text}");
                }

                return value;
            }

            /// <summary>
            /// 尝试解析整数
            /// </summary>
            /// <param name="text">整数文本</param>
            /// <param name="value">解析结果</param>
            /// <returns>是否解析成功</returns>
            private static bool TryParseInt(string text, out int value)
            {
                return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
            }

            /// <summary>
            /// 解析可选浮点数
            /// </summary>
            /// <param name="text">浮点文本</param>
            /// <returns>浮点数</returns>
            private static float ParseOptionalFloat(string text)
            {
                return float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value) ? value : 0f;
            }
        }

        /// <summary>
        /// xlsx 首个工作表读取结果
        /// </summary>
        private sealed class XlsxSheet
        {
            private readonly List<List<string>> rows;

            /// <summary>
            /// 创建 xlsx 工作表数据
            /// </summary>
            /// <param name="rows">行数据</param>
            private XlsxSheet(List<List<string>> rows)
            {
                this.rows = rows;
            }

            /// <summary>
            /// 工作表行数
            /// </summary>
            public int RowCount => this.rows.Count;

            /// <summary>
            /// 读取 xlsx 第一张表
            /// </summary>
            /// <param name="path">xlsx 路径</param>
            /// <returns>工作表数据</returns>
            public static XlsxSheet Load(string path)
            {
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"配置表不存在: {path}", path);
                }

                using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                using ZipArchive archive = new ZipArchive(fileStream, ZipArchiveMode.Read);
                List<string> sharedStrings = ReadSharedStrings(archive);
                ZipArchiveEntry sheetEntry = archive.GetEntry("xl/worksheets/sheet1.xml");
                if (sheetEntry == null)
                {
                    throw new InvalidOperationException($"xlsx 缺少 sheet1: {path}");
                }

                return new XlsxSheet(ReadRows(sheetEntry, sharedStrings));
            }

            /// <summary>
            /// 获取指定单元格
            /// </summary>
            /// <param name="row">行索引</param>
            /// <param name="column">列索引</param>
            /// <returns>单元格文本</returns>
            public string GetCell(int row, int column)
            {
                if (row < 0 || row >= this.rows.Count)
                {
                    return string.Empty;
                }

                List<string> cells = this.rows[row];
                if (column < 0 || column >= cells.Count)
                {
                    return string.Empty;
                }

                return cells[column] ?? string.Empty;
            }

            /// <summary>
            /// 查找指定首列文本所在行
            /// </summary>
            /// <param name="firstCellText">首列文本</param>
            /// <returns>行索引</returns>
            public int FindRow(string firstCellText)
            {
                for (int row = 0; row < this.rows.Count; ++row)
                {
                    if (this.GetCell(row, 0) == firstCellText)
                    {
                        return row;
                    }
                }

                throw new InvalidOperationException($"找不到行: {firstCellText}");
            }

            /// <summary>
            /// 查找必需列
            /// </summary>
            /// <param name="headerRow">表头行</param>
            /// <param name="columnName">列名</param>
            /// <returns>列索引</returns>
            public int FindColumn(int headerRow, string columnName)
            {
                int column = this.FindOptionalColumn(headerRow, columnName);
                if (column < 0)
                {
                    throw new InvalidOperationException($"找不到列: {columnName}");
                }

                return column;
            }

            /// <summary>
            /// 查找可选列
            /// </summary>
            /// <param name="headerRow">表头行</param>
            /// <param name="columnName">列名</param>
            /// <returns>列索引，不存在时返回 -1</returns>
            public int FindOptionalColumn(int headerRow, string columnName)
            {
                List<string> headerCells = this.rows[headerRow];
                for (int column = 0; column < headerCells.Count; ++column)
                {
                    if (headerCells[column] == columnName)
                    {
                        return column;
                    }
                }

                return -1;
            }

            /// <summary>
            /// 读取共享字符串表
            /// </summary>
            /// <param name="archive">xlsx zip 包</param>
            /// <returns>共享字符串列表</returns>
            private static List<string> ReadSharedStrings(ZipArchive archive)
            {
                ZipArchiveEntry entry = archive.GetEntry("xl/sharedStrings.xml");
                if (entry == null)
                {
                    return new List<string>();
                }

                XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
                using Stream stream = entry.Open();
                XDocument document = XDocument.Load(stream);
                return document.Descendants(ns + "si")
                        .Select(si => string.Concat(si.Descendants(ns + "t").Select(t => t.Value)))
                        .ToList();
            }

            /// <summary>
            /// 读取工作表行数据
            /// </summary>
            /// <param name="sheetEntry">工作表 xml 条目</param>
            /// <param name="sharedStrings">共享字符串列表</param>
            /// <returns>行数据</returns>
            private static List<List<string>> ReadRows(ZipArchiveEntry sheetEntry, List<string> sharedStrings)
            {
                XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
                using Stream stream = sheetEntry.Open();
                XDocument document = XDocument.Load(stream);
                List<List<string>> rows = new List<List<string>>();
                foreach (XElement rowElement in document.Descendants(ns + "row"))
                {
                    int rowIndex = int.Parse(rowElement.Attribute("r")?.Value ?? "1", CultureInfo.InvariantCulture) - 1;
                    EnsureRow(rows, rowIndex);
                    foreach (XElement cellElement in rowElement.Elements(ns + "c"))
                    {
                        string cellReference = cellElement.Attribute("r")?.Value;
                        int columnIndex = GetColumnIndex(cellReference);
                        EnsureColumn(rows[rowIndex], columnIndex);
                        rows[rowIndex][columnIndex] = ReadCellValue(cellElement, sharedStrings, ns);
                    }
                }

                return rows;
            }

            /// <summary>
            /// 读取单元格文本
            /// </summary>
            /// <param name="cellElement">单元格元素</param>
            /// <param name="sharedStrings">共享字符串列表</param>
            /// <param name="ns">xlsx 命名空间</param>
            /// <returns>单元格文本</returns>
            private static string ReadCellValue(XElement cellElement, List<string> sharedStrings, XNamespace ns)
            {
                string type = cellElement.Attribute("t")?.Value;
                if (type == "inlineStr")
                {
                    return string.Concat(cellElement.Descendants(ns + "t").Select(t => t.Value));
                }

                string valueText = cellElement.Element(ns + "v")?.Value ?? string.Empty;
                if (type == "s" && int.TryParse(valueText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int sharedStringIndex))
                {
                    return sharedStringIndex >= 0 && sharedStringIndex < sharedStrings.Count ? sharedStrings[sharedStringIndex] : string.Empty;
                }

                if (type == "b")
                {
                    return valueText == "1" ? "TRUE" : "FALSE";
                }

                return valueText;
            }

            /// <summary>
            /// 确保行存在
            /// </summary>
            /// <param name="rows">行集合</param>
            /// <param name="rowIndex">行索引</param>
            private static void EnsureRow(List<List<string>> rows, int rowIndex)
            {
                while (rows.Count <= rowIndex)
                {
                    rows.Add(new List<string>());
                }
            }

            /// <summary>
            /// 确保列存在
            /// </summary>
            /// <param name="cells">列集合</param>
            /// <param name="columnIndex">列索引</param>
            private static void EnsureColumn(List<string> cells, int columnIndex)
            {
                while (cells.Count <= columnIndex)
                {
                    cells.Add(string.Empty);
                }
            }

            /// <summary>
            /// 从单元格引用解析列索引
            /// </summary>
            /// <param name="cellReference">单元格引用</param>
            /// <returns>列索引</returns>
            private static int GetColumnIndex(string cellReference)
            {
                if (string.IsNullOrWhiteSpace(cellReference))
                {
                    return 0;
                }

                int columnIndex = 0;
                foreach (char character in cellReference)
                {
                    if (!char.IsLetter(character))
                    {
                        break;
                    }

                    columnIndex = columnIndex * 26 + (char.ToUpperInvariant(character) - 'A' + 1);
                }

                return columnIndex - 1;
            }
        }

        /// <summary>
        /// Grid authoring 所需的三张配置表数据
        /// </summary>
        private sealed class GridAuthoringTables
        {
            public Dictionary<int, SlotConfigRecord> Slots;
            public Dictionary<int, PuzzleConfigRecord> Puzzles;
            public Dictionary<int, GridConfigRecord> Grids;

            /// <summary>
            /// 按 id 获取 Grid 配置
            /// </summary>
            /// <param name="id">Grid 配置 id</param>
            /// <returns>Grid 配置</returns>
            public GridConfigRecord GetGrid(int id)
            {
                if (!this.Grids.TryGetValue(id, out GridConfigRecord value))
                {
                    throw new InvalidOperationException($"Grid 配置不存在: {id}");
                }

                return value;
            }

            /// <summary>
            /// 按 id 获取 Slot 配置
            /// </summary>
            /// <param name="id">Slot 配置 id</param>
            /// <returns>Slot 配置</returns>
            public SlotConfigRecord GetSlot(int id)
            {
                if (!this.Slots.TryGetValue(id, out SlotConfigRecord value))
                {
                    throw new InvalidOperationException($"Slot 配置不存在: {id}");
                }

                return value;
            }

            /// <summary>
            /// 按 id 获取 Puzzle 配置
            /// </summary>
            /// <param name="id">Puzzle 配置 id</param>
            /// <returns>Puzzle 配置</returns>
            public PuzzleConfigRecord GetPuzzle(int id)
            {
                if (!this.Puzzles.TryGetValue(id, out PuzzleConfigRecord value))
                {
                    throw new InvalidOperationException($"Puzzle 配置不存在: {id}");
                }

                return value;
            }
        }

        private sealed class SlotConfigRecord
        {
            public int Id;
            public string Name;
            public string PrefabPath;
        }

        private sealed class PuzzleConfigRecord
        {
            public int Id;
            public string Name;
            public string PrefabPath;
        }

        private sealed class GridConfigRecord
        {
            public int Id;
            public string Name;
            public int Width;
            public int Height;
            public string PrefabPath;
            public List<GridPuzzleRecord> Puzzles;
            public List<int> SlotList;
        }

        private sealed class GridPuzzleRecord
        {
            public int Id;
            public Vector3 LocalPosition;
        }

        private sealed class GridExportData
        {
            public int Id;
            public string Name;
            public int Width;
            public int Height;
            public string PrefabPath;
            public List<GridPuzzleExportData> Puzzles;
            public List<int> SlotList;
        }

        private sealed class GridPuzzleExportData
        {
            public int Id;
            public Vector3 LocalPosition;
        }
#endif
    }
}
