namespace ET.Client
{
    /// <summary>
    /// 当前场景创建完成后，为其挂载通用输入组件。
    /// </summary>
    [Event(SceneType.Current)]
    public class AfterCreateCurrentScene_AddGameplayInputComponent : AEvent<Scene, AfterCreateCurrentScene>
    {
        /// <summary>
        /// 为当前场景补充通用输入组件。
        /// </summary>
        protected override async ETTask Run(Scene scene, AfterCreateCurrentScene args)
        {
            if (scene.GetComponent<InputComponent>() == null)
            {
                scene.AddComponent<InputComponent>();
            }

            await ETTask.CompletedTask;
        }
    }
}
