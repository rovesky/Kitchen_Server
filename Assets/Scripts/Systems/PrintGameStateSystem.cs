using System;
using FootStone.ECS;
using Unity.Collections;
using Unity.Entities;

namespace FootStone.Kitchen
{
    [DisableAutoCreation]
    public class PrintGameStateSystem : SystemBase
    {
        private const int checkDuration = 1;
        private DateTime lastCheckTime;

        protected override void OnCreate()
        {
            lastCheckTime = DateTime.Now;
        }

        protected override void OnUpdate()
        {

            Entities
                .WithStructuralChanges()
                .ForEach((Entity entity,
                    in GameStateComponent gameState,
                    in Countdown countdown) =>
                {

                    var now = DateTime.Now;
                    var timeSpan = DateTime.Now - lastCheckTime;
                    if ((int) timeSpan.TotalSeconds != checkDuration)
                        return;
                    lastCheckTime = now;

                    FSLog.Info($"GameState：{gameState.State}," +
                               $"countdown:{countdown.Value},IsSceneReady:{gameState.IsSceneReady}");

                  
                }).Run();
        }
    }
}