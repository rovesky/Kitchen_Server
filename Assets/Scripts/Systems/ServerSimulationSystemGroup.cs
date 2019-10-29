using FootStone.ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Scripting;

namespace Assets.Scripts.ECS
{
    public static class StopWatchExtensions
    {
        public static float GetTicksDeltaAsMilliseconds(this System.Diagnostics.Stopwatch stopWatch, long previousTicks)
        {
            return (float)((double)(stopWatch.ElapsedTicks - previousTicks) / FrequencyMilliseconds);
        }

        public static long FrequencyMilliseconds = System.Diagnostics.Stopwatch.Frequency / 1000;
    }


    [UnityEngine.ExecuteAlways]
    public class ServerSimulationSystemGroup : ComponentSystemGroup
    {
        private GameWorld gameWorld;
        private NetworkServerNewSystem networkServerSystem;
        private double nextTickTime = 0;
        private System.Random random;
        private long simStartTime;
        private uint simStartTimeTick;
        private float lastSimTime;

        protected override void OnCreate()
        {

            ConfigVar.Init();
            random = new System.Random();
          
            GameWorld.Active = new GameWorld();
            gameWorld = GameWorld.Active;
            networkServerSystem = World.GetOrCreateSystem<NetworkServerNewSystem>();

            m_systemsToUpdate.Add(World.GetOrCreateSystemE<ResetWorldSystem>());
         //   m_systemsToUpdate.Add();

            m_systemsToUpdate.Add(World.GetOrCreateSystemE<HandleCommandSystem>());


            m_systemsToUpdate.Add(World.GetOrCreateSystemE<SpawnEnemySystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystemE<SpawnPlayerServerSystem>());

            m_systemsToUpdate.Add(World.GetOrCreateSystemE<PlayerFireSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystemE<EnemyFireSystem>());

            m_systemsToUpdate.Add(World.GetOrCreateSystemE<MoveForwardSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystemE<MoveSinSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystemE<MovePositionSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystemE<MoveTargetSystem>());
         //   m_systemsToUpdate.Add(World.GetOrCreateSystemE<MoveTranslationSystem>());

            m_systemsToUpdate.Add(World.GetOrCreateSystemE<CheckOutOfRangeSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystemE<RayCastSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystemE<HealthSystem>());

            m_systemsToUpdate.Add(World.GetOrCreateSystemE<ApplyPresentationSystem>());

            m_systemsToUpdate.Add(World.GetOrCreateSystemE<DespawnServerSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystemE<DespawnSystem>());

       //     m_systemsToUpdate.Add(World.GetOrCreateSystemE<GenSnapshotSystem>());
       //     m_systemsToUpdate.Add(World.GetOrCreateSystemE<SendDataSystem>());
        
        }

        /// <summary>
        /// 不需要排序
        /// </summary>
        public override void SortSystemUpdateList()
        {

        }

        private void ServerTick()
        {
            var worldTime = GetSingleton<WorldTime>();
            worldTime.gameTick.Tick++;
            worldTime.gameTick.TickDuration = worldTime.gameTick.TickInterval;
            //     worldTime.tick.FrameDuration = worldTime.tick.TickInterval;

            networkServerSystem.HandleClientCommands((int)worldTime.gameTick.Tick);

            SetSingleton(worldTime);
            base.OnUpdate();
        }

        protected override void OnUpdate()
        {
            var worldTime = GetSingleton<WorldTime>();

            simStartTime = GameWorld.Active.Clock.ElapsedTicks;
            simStartTimeTick = worldTime.Tick;

            networkServerSystem.Update();

              
            int tickCount = 0;
            while (worldTime.frameTime > nextTickTime)
            {
                tickCount++;
                ServerTick();
                //  if (gameWorld.Tick % 10 == 0)
                //    Thread.Sleep(random.Next(30, 100));
                networkServerSystem.GenerateSnapshot(lastSimTime);
                nextTickTime += worldTime.gameTick.TickInterval;
            }

            float remainTime = (float)(nextTickTime - worldTime.frameTime);

            int rate = worldTime.gameTick.TickRate;
            if (remainTime > 0.75f * worldTime.gameTick.TickInterval)
                rate -= 2;
            else if (remainTime < 0.25f * worldTime.gameTick.TickInterval)
                rate += 2;

            Application.targetFrameRate = rate;

            networkServerSystem.SendData();

            if (simStartTimeTick != worldTime.Tick)
            {
                // Only update sim time if we actually simulatated
                // TODO : remove this when targetFrameRate works the way we want it.
                lastSimTime = GameWorld.Active.Clock.GetTicksDeltaAsMilliseconds(simStartTime);
            }

            //     FSLog.Info($"targetFrameRate:{Application.targetFrameRate}," +
            //  $"Game.frameTime:{worldTime.frameTime},remainTime:{remainTime},tick:{worldTime.tick.Tick}");
        }
    }

    // [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    //public class MyrSimulationSystemGroup : SimulationSystemGroup
    //{
    //    [Preserve] public MyrSimulationSystemGroup() { }
    //    //protected override void OnCreate()
    //    //{
    //    //    FSLog.Info($"MyrSimulationSystemGroup OnCreate");
    //    //    base.OnCreate();
    //    //    //m_systemsToUpdate.Add(World.GetOrCreateSystemE<DespawnSystem>());
    //    //    //m_systemsToUpdate.Add(World.GetOrCreateSystemE<GenSnapshotSystem>());
    //    //    //m_systemsToUpdate.Add(World.GetOrCreateSystemE<SendDataSystem>());
    //    //}


    //}
}
