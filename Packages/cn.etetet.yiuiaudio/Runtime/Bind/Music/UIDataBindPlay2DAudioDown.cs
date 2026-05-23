using ET;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YIUIFramework
{
    /// <summary>
    /// 点击按下后 播放2D音乐
    /// 由UI点击按下后 直接触发
    /// 没有控制开关
    /// </summary>
    [LabelText("Play 音效2D按下")]
    [AddComponentMenu("YIUIBind/Data/音效2D播放按下 【Play2DAudioDown】 AudioUIDataBindPlay2DAudioDown")]
    public sealed class UIDataBindPlay2DAudioDown : UIMusicSelect, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            if (string.IsNullOrEmpty(AudioKey))
            {
                Debug.LogError($"未配置点击音效请检查 {gameObject.name}");
                return;
            }

            AudioMgr.Inst.Play2DAudio(AudioKey, m_Volume, m_IsConfig);
        }
    }
}