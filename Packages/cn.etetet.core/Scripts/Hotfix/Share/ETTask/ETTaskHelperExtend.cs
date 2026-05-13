using System;

namespace ET
{
    /// <summary>
    /// 提供依赖 Hotfix 层 TimerComponent 的通用 ETTask 等待扩展。
    /// </summary>
    public static class ETTaskHelperExtend
    {
        /// <summary>
        /// 在当前 Entity 所属 Root 的 TimerComponent 上逐帧等待，直到条件满足或实体上下文失效。
        /// </summary>
        /// <param name="self">用于定位 Root 和 TimerComponent 的实体。</param>
        /// <param name="func">每帧检查的完成条件。</param>
        public static async ETTask WaitUntil(this Entity self, Func<bool> func)
        {
            TimerComponent timer = self?.Root()?.GetComponent<TimerComponent>();
            if (timer == null)
            {
                return;
            }

            await timer.WaitUntil(func);
        }

        /// <summary>
        /// 通过 TimerComponent 逐帧等待，直到条件满足、Timer 被释放或条件抛出异常。
        /// </summary>
        /// <param name="self">负责逐帧等待的计时器组件。</param>
        /// <param name="func">每帧检查的完成条件。</param>
        public static async ETTask WaitUntil(this TimerComponent self, Func<bool> func)
        {
            EntityRef<TimerComponent> timer = self;

            while (true)
            {
                if (timer.Entity == null)
                {
                    return;
                }

                await timer.Entity.WaitFrameAsync();

                if (func == null)
                {
                    return;
                }

                try
                {
                    if (func.Invoke())
                    {
                        return;
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"WaitUntil Error: {e}");
                    return;
                }
            }
        }
    }
}
