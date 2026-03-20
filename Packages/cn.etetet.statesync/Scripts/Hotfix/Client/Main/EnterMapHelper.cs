using System;
using Unity.Mathematics;

namespace ET.Client
{
    public static partial class EnterMapHelper
    {
        public static async ETTask EnterMapAsync(Scene root)
        {
            try
            {
                // 单机模式下从进图入口源头禁用 EnterMap 网络请求，保留原代码方便后续恢复。
                /*
                G2C_EnterMap g2CEnterMap = await root.GetComponent<ClientSenderComponent>().Call(C2G_EnterMap.Create()) as G2C_EnterMap;
                // 等待场景切换完成
                await root.GetComponent<ObjectWait>().Wait<Wait_SceneChangeFinish>();
                EventSystem.Instance.Publish(root, new EnterMapFinish());
                */

                await EnterMapLocalAsync(root);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private static async ETTask EnterMapLocalAsync(Scene root)
        {
            Log.Info("single-player mode: entering local map without network.");

            const string localSceneName = "Test";

            CurrentScenesComponent currentScenesComponent = root.GetComponent<CurrentScenesComponent>();
            currentScenesComponent.Scene?.Dispose();

            long sceneInstanceId = IdGenerater.Instance.GenerateInstanceId();
            Scene currentScene = CurrentSceneFactory.Create(sceneInstanceId, localSceneName, currentScenesComponent);
            UnitComponent unitComponent = currentScene.AddComponent<UnitComponent>();

            // 保持原有事件顺序，先走场景切换开始事件，让本地场景资源完成加载。
            await EventSystem.Instance.PublishAsync(root, new SceneChangeStart());
            
            EventSystem.Instance.Publish(currentScene, new SceneChangeFinish());
            
            root.GetComponent<ObjectWait>().Notify(new Wait_SceneChangeFinish());
            
            EventSystem.Instance.Publish(root, new EnterMapFinish());
            
            await ETTask.CompletedTask;
        }
    }
}
