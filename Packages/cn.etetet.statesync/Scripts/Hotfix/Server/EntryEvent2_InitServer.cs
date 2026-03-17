namespace ET.Server
{
    [Event(SceneType.StateSync)]
    public class EntryEvent2_InitServer: AEvent<Scene, EntryEvent2>
    {
        protected override async ETTask Run(Scene root, EntryEvent2 args)
        {
            // 单机模式下从服务端启动源头禁用 NetInner/Router/Realm/Gate/Map 等 Fiber 创建，保留原代码方便后续恢复。
            /*
            //按配置扩展更多场景/进程内 Fiber
            if (Options.Instance.Console == 1)
            {
                root.AddComponent<ConsoleComponent>();
            }
            
            World.Instance.AddSingleton<NavmeshComponent>();
            
            int process = root.Fiber.Process;
            StartProcessConfig startProcessConfig = StartProcessConfigCategory.Instance.Get(process);
            if (startProcessConfig.Port != 0)
            {
                await FiberManager.Instance.Create(SchedulerType.ThreadPool, SceneType.NetInner, 0, SceneType.NetInner, "NetInner");
            }

            // 根据配置创建纤程
            var scenes = StartSceneConfigCategory.Instance.GetByProcess(process);
            
            foreach (StartSceneConfig startConfig in scenes)
            {
                int sceneType = SceneTypeSingleton.Instance.GetSceneType(startConfig.SceneType);
                await FiberManager.Instance.Create(SchedulerType.ThreadPool, startConfig.Id, startConfig.Zone, sceneType, startConfig.Name);
            }
            */

            Log.Info("single-player mode: EntryEvent2_InitServer network/server startup disabled.");
            await ETTask.CompletedTask;
        }
    }
}
