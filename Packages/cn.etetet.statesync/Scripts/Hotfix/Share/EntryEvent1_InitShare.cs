using Unity.Mathematics;

namespace ET
{
    [Event(SceneType.StateSync)]
    public class EntryEvent1_InitShare: AEvent<Scene, EntryEvent1>
    {
        protected override async ETTask Run(Scene root, EntryEvent1 args)
        {
            //计时器系统
            root.AddComponent<TimerComponent>();
            
            //协程锁
            root.AddComponent<CoroutineLockComponent>();
            
            //等待/回调对象
            root.AddComponent<ObjectWait>();
            
            //串行消息队列
            root.AddComponent<MailBoxComponent, int>(MailBoxType.UnOrderedMessage);
            
            //进程内通信发送器
            root.AddComponent<ProcessInnerSender>();
            
            MongoRegister.RegisterStruct<float2>();
            MongoRegister.RegisterStruct<float3>();
            MongoRegister.RegisterStruct<float4>();
            MongoRegister.RegisterStruct<quaternion>();
            
            await ETTask.CompletedTask;
        }
    }
}