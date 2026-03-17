using System;
using CommandLine;
using UnityEngine;

namespace ET
{
    public class Init: MonoBehaviour
    {
        private void Start()
        {
            this.StartAsync().NoContext();
        }
		
        private async ETTask StartAsync()
        {
            DontDestroyOnLoad(gameObject);
			
            //注册UnhandledException 把未捕获异常打进ET日志 避免启动期异常直接静默
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Log.Error(e.ExceptionObject.ToString());
            };

            // 创建并注册Options
            // 命令行参数
            string[] args = "".Split(" ");
            Parser.Default.ParseArguments<Options>(args)
                    .WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
                    .WithParsed((o)=>World.Instance.AddSingleton(o));

            GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
            Options.Instance.SceneName = globalConfig.SceneName;
			
            //初始化 Logger
            World.Instance.AddSingleton<Logger>().Log = new UnityLogger();
            ETTask.ExceptionHandler += Log.Error;
			
            //初始化 TimeInfo FiberManager
            World.Instance.AddSingleton<TimeInfo>();
            World.Instance.AddSingleton<FiberManager>();

            //创建 YooAssets 默认资源包 DefaultPackage
            await World.Instance.AddSingleton<ResourcesComponent>().CreatePackageAsync("DefaultPackage", true);
            
            World.Instance.AddSingleton<CodeLoader>().Start().NoContext();
        }

        private void Update()
        {
            TimeInfo.Instance.Update();
            FiberManager.Instance.Update();
        }

        private void LateUpdate()
        {
            FiberManager.Instance.LateUpdate();
        }

        private void OnApplicationQuit()
        {
            World.Instance.Dispose();
        }
    }
}