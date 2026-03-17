using System;
using System.Collections.Generic;
using System.IO;
using YIUIFramework;

namespace ET.Client
{
    [Event(SceneType.StateSync)]
    public class EntryEvent3_InitClient : AEvent<Scene, EntryEvent3>
    {
        protected override async ETTask Run(Scene root, EntryEvent3 args)
        {
            //缓存 Global、GlobalUI、GlobalConfig 等全局 Unity 节点和配置
            root.AddComponent<GlobalComponent>();
            
            //业务期资源加载器
            root.AddComponent<ResourcesLoaderComponent>();
            
            //当前玩家相关数据挂点
            root.AddComponent<PlayerComponent>();
            
            //当前客户端场景集合/引用管理
            root.AddComponent<CurrentScenesComponent>();

            //创建 YIUIMgrComponent，让 UI 框架真正启动
            var result = await root.AddComponent<YIUIMgrComponent>().Initialize();
            if (!result)
            {
                Log.Error("初始化UI失败");
                return;
            }

            await EventSystem.Instance.PublishAsync(root, new AppStartInitFinish());
        }
    }
}