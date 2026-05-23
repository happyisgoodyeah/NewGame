using System;
using UnityEngine;
using YIUIFramework;

namespace ET
{
    [EnableClass]
    public class AudioClipLoader : IAudioClipLoader
    {
        private readonly EntityRef<Entity> m_EntityRef;

        public AudioClipLoader(Entity entity)
        {
            m_EntityRef = entity;
        }

        public void UnloadAudioClip(AudioClip clip)
        {
            EventSystem.Instance?.YIUIInvokeEntitySyncSafety(m_EntityRef, new YIUIInvokeEntity_Release { obj = clip });
        }

        public async ETTask<AudioClip> LoadAudioClipAsync(string clipName)
        {
            var audioClip = await EventSystem.Instance?.YIUIInvokeEntityAsyncSafety<YIUIInvokeLoadAudioClip, ETTask<AudioClip>>(m_EntityRef, new YIUIInvokeLoadAudioClip
            {
                ResName = clipName,
            });

            if (audioClip == null)
            {
                Debug.LogError($"音频: {clipName} 没有找到!");
                return null;
            }

            return audioClip;
        }
    }
}
