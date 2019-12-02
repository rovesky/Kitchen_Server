using FootStone.ECS;
using Unity.Entities;

namespace FootStone.Kitchen
{
    [DisableAutoCreation]
    public class HandleCommandsSystem : ComponentSystem
    {

        protected override void OnUpdate()
        {
            var tick = GetSingleton<WorldTime>().Tick;
            Entities.ForEach((Entity e, ref UserCommand userCommand) =>
            {
                var buffer = EntityManager.GetBuffer<UserCommandBuffer>(e);
                if (buffer.Length == 0)
                    return;


                var removeCount = 0;
                for (var i = 0; i < buffer.Length; ++i)
                {
                    if (buffer[i].command.checkTick <= tick)
                        removeCount++;
                    //获取当前serverTick的command
                    if (buffer[i].command.checkTick == tick)
                    {
                        //  FSLog.Info($"use command:{buffer[i].command.renderTick},{buffer[i].command.checkTick},{tick}");
                        userCommand = buffer[i].command;
                    }
                }

                //删除小于等于当前serverTick的command
                buffer.RemoveRange(0, removeCount);
            });
        }

    }
}
