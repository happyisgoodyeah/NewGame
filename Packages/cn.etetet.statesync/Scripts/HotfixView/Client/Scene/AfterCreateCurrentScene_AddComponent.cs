namespace ET.Client
{
    /// <summary>
    /// 当前场景创建完成后的统一初始化入口，集中挂载基础组件
    /// </summary>
    [Event(SceneType.Current)]
    public class AfterCreateCurrentScene_AddComponent: AEvent<Scene, AfterCreateCurrentScene>
    {
        /// <summary>
        /// 为当前场景挂载运行所需组件
        /// </summary>
        protected override ETTask Run(Scene scene, AfterCreateCurrentScene args)
        {
            if (scene.GetComponent<UIComponent>() == null)
            {
                scene.AddComponent<UIComponent>();
            }

            if (scene.GetComponent<ResourcesLoaderComponent>() == null)
            {
                scene.AddComponent<ResourcesLoaderComponent>();
            }

            if (scene.GetComponent<InputComponent>() == null)
            {
                scene.AddComponent<InputComponent>();
            }

            return ETTask.CompletedTask;
        }
    }
}
