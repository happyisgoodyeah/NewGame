/*using UnityEditor;
using UnityEngine;

namespace YIUIFramework.Editor
{
    [CustomEditor(typeof(AudioClip), true)]
    public class AudioClipPreviewEditor : UnityEditor.Editor
    {
        private static GameObject previewObj;
        private static AudioSource previewSource;
        private static AudioClip lastClip;
        private static float soundVolume = 1f;
        private static float newClipProgress = 0f;
        private static bool loopSelection = false;
        private static AudioClip selectedClip;
        private static AudioClipPreviewEditor currentInspector;

        public override void OnInspectorGUI()
        {
            // 记录当前Inspector实例，便于刷新
            currentInspector = this;
            GUI.enabled = true; // 确保本Editor控件可用
            var clip = target as AudioClip;
            if (clip == null)
            {
                EditorGUILayout.HelpBox("当前选中对象不是AudioClip资源，无法预览。请选中音频文件。", MessageType.Warning);
                return;
            }

            selectedClip = clip;
            GUILayout.Space(8);

            // 音频信息区
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("音频预览", EditorStyles.boldLabel);
            if (clip != null)
            {
                GUILayout.Label($"名称：{clip.name}");
                GUILayout.Label($"时长：{clip.length:F2} 秒");
                GUILayout.Label($"采样率：{clip.frequency} Hz");
                GUILayout.Label($"通道数：{clip.channels}");
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(8);

            // 播放/停止按钮
            EditorGUILayout.BeginHorizontal();
            GUIStyle playBtn = new GUIStyle(GUI.skin.button);
            playBtn.fixedHeight = 40;
            playBtn.fixedWidth = 40;
            playBtn.fontSize = 18;
            string playBtnLabel = "▶";

            if (previewSource != null)
            {
                playBtnLabel = "■";
            }

            if (GUILayout.Button(playBtnLabel, playBtn))
            {
                if (previewSource == null)
                {
                    PlayPreview(clip);
                }
                else
                {
                    StopPreview();
                }
            }

            // 暂停/恢复按钮
            GUIStyle pauseBtn = new GUIStyle(GUI.skin.button);
            pauseBtn.fixedHeight = 40;
            pauseBtn.fixedWidth = 60;
            pauseBtn.fontSize = 14;
            bool pauseBtnEnabled = previewSource != null;
            EditorGUI.BeginDisabledGroup(!pauseBtnEnabled);
            string pauseBtnLabel = pauseBtnEnabled ? previewSource.isPlaying ? "暂停" : "恢复" : "暂停";
            if (GUILayout.Button(pauseBtnLabel, pauseBtn))
            {
                if (previewSource.isPlaying)
                {
                    previewSource.Pause();
                }
                else
                {
                    previewSource.UnPause();
                }
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(8);

            // 进度条
            float length = clip != null ? clip.length : 0f;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"{newClipProgress:F2} s", GUILayout.Width(60));
            newClipProgress = GUILayout.HorizontalSlider(newClipProgress, 0, length, GUILayout.Width(150));
            
            Debug.LogError($"11: {newClipProgress} ");
            GUILayout.Label($"{length:F2} s", GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(8);

            // 音量
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("音量", GUILayout.Width(60));
            float newVolume = GUILayout.HorizontalSlider(soundVolume, 0, 1, GUILayout.Width(150));
            GUILayout.Label($"{Mathf.RoundToInt(newVolume * 100)}%", GUILayout.Width(60));
            if (Mathf.Abs(newVolume - soundVolume) > 0.001f)
            {
                soundVolume = newVolume;
                if (previewSource != null)
                {
                    previewSource.volume = soundVolume;
                }
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(8);

            // 选项按钮区
            EditorGUILayout.BeginHorizontal();
            GUIStyle toggleBtn = new GUIStyle(GUI.skin.button);
            toggleBtn.fixedHeight = 32;
            toggleBtn.fixedWidth = 120;
            toggleBtn.fontSize = 13;
            if (GUILayout.Button($"循环播放: {(loopSelection ? "开" : "关")}", toggleBtn))
            {
                loopSelection = !loopSelection;
                if (previewSource != null)
                {
                    previewSource.loop = loopSelection;
                }
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(8);
        }

        private static void UpdateInspector()
        {
            currentInspector?.Repaint();
            if (previewSource == null)
            {
                StopPreview();
            }
            else
            {
                if (!previewSource.isPlaying)
                {
                    Debug.LogError($"暂停中修改 {newClipProgress}");
                    previewSource.time = newClipProgress;
                }
                else
                {
                    Debug.LogError($"非暂停同步  {newClipProgress}");
                    newClipProgress = previewSource.time;
                }

                if (!previewSource.loop && previewSource.time >= previewSource.clip.length)
                {
                    Debug.LogError($"结束");

                    StopPreview();
                }
            }
        }

        private static void PlayPreview(AudioClip clip)
        {
            StopPreview();
            if (clip == null) return;

            Debug.LogError($"PlayPreview");

            EditorUtility.audioMasterMute = false;
            previewObj = new GameObject("YIUI_AudioClipPreviewObj");
            previewObj.hideFlags = HideFlags.HideAndDontSave;
            previewSource = previewObj.AddComponent<AudioSource>();
            previewSource.clip = clip;
            previewSource.playOnAwake = false;
            previewSource.volume = soundVolume;
            previewSource.loop = loopSelection;
            previewSource.time = newClipProgress;
            previewSource.Play();
            lastClip = clip;
            EditorApplication.update += UpdateInspector;
        }

        private static void StopPreview()
        {
            Debug.LogError($"StopPreview");
            if (previewSource != null)
            {
                previewSource.Stop();
            }

            if (previewObj != null)
            {
                GameObject.DestroyImmediate(previewObj);
            }

            EditorApplication.update -= UpdateInspector;
            previewObj = null;
            previewSource = null;
            lastClip = null;
            newClipProgress = 0f;
            currentInspector = null;
        }

        void OnDisable()
        {
            StopPreview();
        }
    }
}*/