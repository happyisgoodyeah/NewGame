using System;

namespace ET
{
    public struct EntryEvent1
    {
    }   
    
    public struct EntryEvent2
    {
    } 
    
    public struct EntryEvent3
    {
    }
    
    public static class Entry
    {
        public static void Init()
        {
            
        }
        
        public static void Start()
        {
            StartAsync().NoContext();
        }
        
        private static async ETTask StartAsync()
        {
            //时间精度相关初始化
            WinPeriod.Init();

            // 注册Mongo type 注册 BSON/Mongo 相关类型
            MongoRegister.Init();
            
            //注册 MemoryPack 序列化
            MemoryPackRegister.Init();
            
            // 注册Entity序列化器
            EntitySerializeRegister.Init();

            //建立 SceneType int <-> 名字 映射
            World.Instance.AddSingleton<SceneTypeSingleton, Type>(typeof(SceneType));
            
            //对象池
            World.Instance.AddSingleton<ObjectPool>();
            
            //ID 生成
            World.Instance.AddSingleton<IdGenerater>();
            
            //消息号与类型映射
            World.Instance.AddSingleton<OpcodeType>();
            
            //消息派发队列
            World.Instance.AddSingleton<MessageQueue>();
            
            //网络服务集合
            World.Instance.AddSingleton<NetServices>();
            
            //消息日志过滤，这里忽略了 ping
            LogMsg logMsg = World.Instance.AddSingleton<LogMsg>();
            logMsg.AddIgnore(LoginOuter.C2G_Ping);
            logMsg.AddIgnore(LoginOuter.G2C_Ping);
            
            // 创建需要reload的code singleton
            CodeTypes.Instance.CodeProcess();
            
            //加载配置 ConfigLoader.LoadAsync 在业务启动前，把所有配置单例先准备好
            await World.Instance.AddSingleton<ConfigLoader>().LoadAsync();
            
            //创建主 Fiber
            await FiberManager.Instance.Create(SchedulerType.Main, SceneType.Main, 0, SceneType.Main, "");
        }
    }
}