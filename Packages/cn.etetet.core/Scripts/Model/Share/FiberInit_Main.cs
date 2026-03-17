namespace ET
{
    [Invoke(SceneType.Main)]
    public class FiberInit_Main: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            //根 Scene 的 SceneType 从 Main 改成 Options.SceneName 对应的 StateSync
            
            Scene root = fiberInit.Fiber.Root;
            
            int sceneType = SceneTypeSingleton.Instance.GetSceneType(Options.Instance.SceneName);
            root.SceneType = sceneType;
            
            await EventSystem.Instance.PublishAsync(root, new EntryEvent1());
            await EventSystem.Instance.PublishAsync(root, new EntryEvent2());
            await EventSystem.Instance.PublishAsync(root, new EntryEvent3());
        }
    }
}