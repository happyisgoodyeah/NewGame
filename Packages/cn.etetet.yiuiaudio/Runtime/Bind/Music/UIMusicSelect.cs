using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace YIUIFramework
{
    [AddComponentMenu("")]
    public class UIMusicSelect : SerializedMonoBehaviour
    {
        #if UNITY_EDITOR
        [HorizontalGroup("Audio", Width = 55f)]
        [VerticalGroup("Audio/Left")]
        [BoxGroup("Audio/Left/音频", centerLabel: true)]
        [PropertyOrder(int.MinValue)]
        [HideLabel]
        [ShowInInspector]
        [PreviewField(38, ObjectFieldAlignment.Center)]
        [OnValueChanged("OnValueChanged")]
        [NonSerialized]
        [Required("")]
        [DisableIf(nameof(m_IsConfig))]
        private AudioClip m_SourceObject;

        [BoxGroup("Audio/Left/音频", centerLabel: true)]
        [HorizontalGroup("Audio/Left/音频/Music")]
        [Button("", Icon = SdfIconType.PlayFill, IconAlignment = IconAlignment.LeftOfText)]
        [ShowIf(nameof(m_SourceObject))]
        private void PlayMusic()
        {
            StopMusic();

            var audioUtilType = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");

            if (audioUtilType != null)
            {
                var playMethod = audioUtilType.GetMethod("PlayPreviewClip",
                    BindingFlags.Static | BindingFlags.Public,
                    null,
                    new[] { typeof(AudioClip), typeof(int), typeof(bool) },
                    null);
                if (playMethod == null)
                {
                    Debug.LogError($"没有找到 AudioUtil.PlayPreviewClip 方法");
                    return;
                }

                playMethod.Invoke(null, new object[] { m_SourceObject, 0, false });
            }
            else
            {
                Debug.LogError("没有找到 AudioUtil 类");
            }
        }

        [BoxGroup("Audio/Left/音频", centerLabel: true)]
        [HorizontalGroup("Audio/Left/音频/Music")]
        [Button("", Icon = SdfIconType.PauseFill, IconAlignment = IconAlignment.LeftOfText)]
        [ShowIf(nameof(m_SourceObject))]
        private void StopMusic()
        {
            var audioUtilType = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");

            if (audioUtilType != null)
            {
                var stopAllMethod = audioUtilType.GetMethod("StopAllPreviewClips",
                    BindingFlags.Static | BindingFlags.Public);
                if (stopAllMethod == null)
                {
                    Debug.LogError($"没有找到 AudioUtil.StopAllPreviewClips 方法");
                    return;
                }

                stopAllMethod.Invoke(null, null);
            }
            else
            {
                Debug.LogError("没有找到 AudioUtil 类");
            }
        }
        #endif

        [PropertyOrder(int.MinValue)]
        [LabelWidth(50)]
        [LabelText("启用配置")]
        [BoxGroup("Audio/Right/信息", centerLabel: true)]
        [VerticalGroup("Audio/Right")]
        [OnValueChanged("OnValueChangedIsConfig")]
        [ToggleLeft]
        public bool m_IsConfig;

        [PropertyOrder(int.MinValue)]
        [LabelWidth(50)]
        [LabelText("音效名称")]
        [BoxGroup("Audio/Right/信息", centerLabel: true)]
        [VerticalGroup("Audio/Right")]
        [ShowIf("Show")]
        [ReadOnly]
        public string m_MusicName;

        [PropertyOrder(int.MinValue)]
        [Range(0f, 1f)]
        [LabelText("音量")]
        [LabelWidth(50)]
        [BoxGroup("Audio/Right/信息", centerLabel: true)]
        [VerticalGroup("Audio/Right")]
        [HideIf(nameof(m_IsConfig))]
        public float m_Volume = 1;

        [PropertyOrder(int.MinValue)]
        [LabelWidth(50)]
        [LabelText("配置Key")]
        [EnableIf("@UIOperationHelper.CommonShowIf()")]
        [ValueDropdown("GetKeyTypesSelectList", DoubleClickToConfirm = true)]
        [OnValueChanged("OnKeyValueChanged")]
        [BoxGroup("Audio/Right/信息", centerLabel: true)]
        [VerticalGroup("Audio/Right")]
        [ShowIf(nameof(m_IsConfig))]
        public string m_ConfigKey;

        [SerializeField]
        [HideInInspector]
        private string m_LastMusicName;

        public string AudioKey
        {
            get
            {
                if (m_IsConfig)
                {
                    return m_ConfigKey;
                }
                else
                {
                    return m_MusicName;
                }
            }
        }

        #if UNITY_EDITOR

        private void OnValueChangedIsConfig()
        {
            StopMusic();
            if (m_IsConfig)
            {
                OnKeyValueChanged();
            }
            else
            {
                m_MusicName = m_LastMusicName;
                m_SourceObject = GetSourceObject();
            }
        }

        private void OnValueChanged()
        {
            if (m_SourceObject == null)
            {
                m_MusicName = "";
                return;
            }

            m_MusicName = m_SourceObject.name;
            m_LastMusicName = m_MusicName;
            m_SourceObject = GetSourceObject(false);
            if (m_SourceObject == null)
            {
                UnityTipsHelper.ShowErrorContext(gameObject, $"资源 [{m_MusicName}] 不是音频 请选择一个音频!!!");
                m_MusicName = "";
            }
        }

        private bool Show()
        {
            if (string.IsNullOrEmpty(m_MusicName))
            {
                m_SourceObject = null;
                return true;
            }

            if (m_SourceObject != null)
            {
                return true;
            }

            m_SourceObject = GetSourceObject();
            return true;
        }

        //这个方法也可以作为外部调用统一检查 是否有无效配置
        //TODO 做一个可视化的检查工具
        public AudioClip GetSourceObject(bool logError = true)
        {
            if (string.IsNullOrEmpty(m_MusicName))
            {
                Debug.LogError($"{gameObject.name} 音乐名称 资源名称不能为空 请选择一个音频!!!", gameObject);
                return null;
            }

            var assets = UnityEditor.AssetDatabase.FindAssets($"{m_MusicName} t:AudioClip", null);
            if (assets == null || assets.Length == 0)
            {
                if (logError)
                {
                    Debug.LogError($"{gameObject.name} 找不到资源: {m_MusicName}", gameObject);
                }

                return null;
            }

            AudioClip resObj = null;
            var paths = new List<string>();
            foreach (var guid in assets)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (obj == null)
                {
                    if (logError)
                    {
                        Debug.LogError($"{gameObject.name} 找不到资源: {m_MusicName} path: {path}", gameObject);
                    }
                }
                else
                {
                    if (obj.name == m_MusicName)
                    {
                        resObj = obj;
                        paths.Add(path);
                    }
                }
            }

            if (paths.Count > 1)
            {
                var errorTips = $"{gameObject.name} 找到多个资源: {m_MusicName} 请确保资源名称唯一";
                foreach (var path in paths)
                {
                    errorTips += $"\n{path}";
                }

                Debug.LogError(errorTips, gameObject);
            }

            if (resObj != null)
            {
                return resObj;
            }

            if (logError)
            {
                Debug.LogError($"{gameObject.name} 找不到资源: {m_MusicName}", gameObject);
            }

            return null;
        }

        private IEnumerable GetKeyTypesSelectList()
        {
            var showValues = new ValueDropdownList<string>();
            var keys = YIUIAudioHelper.GetKeys();

            foreach (var key in keys)
            {
                showValues.Add(YIUIAudioHelper.GetDisplayDesc(key), key);
            }

            return YIUIAudioHelper.SubDisplayValueDropdownList(showValues);
        }

        private void OnKeyValueChanged()
        {
            if (string.IsNullOrEmpty(m_ConfigKey))
            {
                m_MusicName = null;
            }
            else
            {
                m_MusicName = YIUIAudioHelper.GetConfigAudioName(m_ConfigKey);
            }

            m_SourceObject = GetSourceObject();
        }
        #endif
    }
}