namespace ET.Client
{
    public static class LoginHelper
    {
        public static async ETTask Login(Scene root, string address, string account, string password)
        {
            // 单机模式下从登录入口源头禁用网络登录，保留原代码方便后续恢复。
            /*
            root.RemoveComponent<ClientSenderComponent>();
            
            ClientSenderComponent clientSenderComponent = root.AddComponent<ClientSenderComponent>();
            
            long playerId = await clientSenderComponent.LoginAsync(address, account, password);
            root.GetComponent<PlayerComponent>().MyId = playerId;
            */
            
            root.GetComponent<PlayerComponent>().MyId = 1;
            Log.Info("single-player mode: login network flow disabled, using local player id.");
            
            await EventSystem.Instance.PublishAsync(root, new LoginFinish());
        }
    }
}
