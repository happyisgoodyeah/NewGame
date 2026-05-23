using ET;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YIUIFramework
{
    /// <summary>
    /// 自动播放音效2D
    /// OnEnable直接触发
    /// 没有控制开关
    /// </summary>
    [LabelText("Play 音效2D播放自动")]
    [AddComponentMenu("YIUIBind/Data/音效2D播放自动 【Play2DAudioAuto】 AudioUIDataBindPlayAuto2DAudio")]
    public sealed class UIDataBindPlay2DAudioAuto : UIMusicSelect
    {
        [SerializeField]
        [LabelText("延迟播放时间(秒)")]
        private float m_WaitPlay = 0;

        [SerializeField]
        [LabelText("隐藏时播放")]
        private bool m_DisablePlay = false;

        private void OnEnable()
        {
            if (m_DisablePlay) return;
            TryPlay();
        }

        private void OnDisable()
        {
            if (!m_DisablePlay) return;
            TryPlay();
        }

        private void TryPlay()
        {
            if (string.IsNullOrEmpty(AudioKey))
            {
                Debug.LogError($"未配置点击音效请检查 {gameObject.name}");
                return;
            }

            if (m_WaitPlay <= 0)
            {
                AudioMgr.Inst.Play2DAudio(AudioKey, m_Volume, m_IsConfig);
            }
            else
            {
                AudioMgr.Inst.Play2DAudioWait(m_WaitPlay, AudioKey, m_Volume, m_IsConfig).NoContext();
            }
        }
    }
}