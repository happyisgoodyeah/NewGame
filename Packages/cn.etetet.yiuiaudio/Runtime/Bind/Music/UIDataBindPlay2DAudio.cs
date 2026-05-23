using ET;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YIUIFramework
{
    /// <summary>
    /// 点击就 播放2D音乐
    /// 由UI点击 直接触发
    /// 没有控制开关
    /// </summary>
    [LabelText("Play 音效2D")]
    [AddComponentMenu("YIUIBind/Data/音效2D播放点击 【Play2DAudio】 AudioUIDataBindPlay2DAudio")]
    public sealed class UIDataBindPlay2DAudio : UIMusicSelect, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
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