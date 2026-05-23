#if UNITY_EDITOR
namespace ET
{
    public static class AudioConfigHelper
    {
        public static string GetAudioName(string audioKey, int index = 0)
        {
            var audioConfigCategory = LubanEditorConfigCategory.Get<AudioConfigCategory>();
            if (audioConfigCategory == null)
            {
                Log.Error($"找不到 AudioConfigCategory。");
                return null;
            }

            var config = audioConfigCategory.Get(audioKey);
            if (config == null)
            {
                Log.Error($"找不到 {audioKey} 的配置。");
                return null;
            }

            var audioCount = config.AudioNames.Count;

            if (audioCount == 0)
            {
                Log.Error($"音频配置 {audioKey} 中没有音频。");
                return null;
            }

            if (index < 0 || index >= audioCount)
            {
                Log.Error($"音频索引 {index} 越界。目前长度: {audioCount}。");
                return null;
            }

            return config.AudioNames[index];
        }
    }
}
#endif