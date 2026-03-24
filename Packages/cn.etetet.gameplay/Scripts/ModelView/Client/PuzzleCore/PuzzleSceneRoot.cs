using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 缓存当前 Unity 场景中 PuzzleCore 相关根节点引用的组件。
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class PuzzleSceneRoot : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 当前场景中的总根节点。
        /// </summary>
        public Transform SceneRoot { get; set; }

        /// <summary>
        /// 当前场景中用于挂载 Grid 表现层对象的根节点。
        /// </summary>
        public Transform GridRoot { get; set; }

        /// <summary>
        /// 当前根节点引用是否已经完成初始化。
        /// </summary>
        public bool IsInitialized { get; set; }
    }
}
