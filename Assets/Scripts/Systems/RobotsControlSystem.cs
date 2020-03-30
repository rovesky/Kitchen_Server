using FootStone.ECS;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;


namespace FootStone.Kitchen
{
    [DisableAutoCreation]
    public class RobotsControlSystem : SystemBase
    {

        private Random random;
        private float x;
        private float z;

        protected override void OnCreate()
        {
            random = new Random(10);
        }

        protected override void OnUpdate()
        {
            Entities
                .WithStructuralChanges()
                .ForEach((Entity entity, in Robot robot) =>
                {
                    var serverTick = GetSingleton<WorldTime>().Tick;
                    var commandBuffer = new UserCommandBuffer();

                    if (serverTick % 100 == 0)
                    {
                        x = random.NextFloat(-1, 1);
                        z = random.NextFloat(-1, 1);
                    }
                    if (serverTick % 100 < 10)
                    {
                        commandBuffer.Command.TargetDir =
                            (new Vector3(x, 0, z)).normalized;
                    }

                    //  commandBuffer.Command.Buttons.Set(UserCommand.Button.Jump, true);
                    commandBuffer.Command.CheckTick = serverTick + 1;
                    EntityManager.GetBuffer<UserCommandBuffer>(entity).Add(commandBuffer);
                }).Run();
        }
    }
}