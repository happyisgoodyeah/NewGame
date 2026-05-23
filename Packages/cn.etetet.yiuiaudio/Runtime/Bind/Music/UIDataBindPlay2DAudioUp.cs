using ET;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YIUIFramework
{
    /// <summary>
    /// 点击抬起后 播放2D音乐
    /// 由UI点击抬起后 直接触发
    /// 没有控制开关
    /// </summary>
    [LabelText("Play 音效2D抬起")]
    [AddComponentMenu("YIUIBind/Data/音效2D播放抬起 【Play2DAudioUp】 AudioUIDataBindPlay2DAudioUp")]
    public sealed class UIDataBindPlay2DAudioUp : UIMusicSelect, IPointerUpHandler
    {
        public void OnPointerUp(PointerEventData eventData)
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