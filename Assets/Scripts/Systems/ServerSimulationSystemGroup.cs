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
    [UnityEngine.ExecuteAlways]
    public class ServerSimulationSystemGroup : ComponentSystemGroup
    {
        private GameWorld gameWorld;
        private double nextTickTime = 0;
        private System.Random random;

        protected override void OnCreate()
        {
            random = new System.Random();
          
            GameWorld.Active = new GameWorld();
            gameWorld = GameWorld.Active;

            m_systemsToUpdate.Add(World.GetOrCreateSystemE<ResetWorldSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystemE<NetworkServerSystem>());

            m_systemsToUpdate.Add(World.GetOrCreateSystemE<SpawnEnemySystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystemE<SpawnPlayerServerSystem>());

            m_systemsToUpdate.Add(World.GetOrCreateSystemE<PlayerFireSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystemE<EnemyFireSystem>());

            m_systemsToUpdate.Add(World.GetOrCreateSystemE<MoveForwardSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystemE<MoveSinSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystemE<MovePositionSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystemE<MoveTargetSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystemE<MoveTranslationSystem>());

            m_systemsToUpdate.Add(World.GetOrCreateSystemE<CheckOutOfRangeSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystemE<RayCastSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystemE<HealthSystem>());

            m_systemsToUpdate.Add(World.GetOrCreateSystemE<DespawnSystem>());

            m_systemsToUpdate.Add(World.GetOrCreateSystemE<GenSnapshotSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystemE<SendDataSystem>());
        
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
            worldTime.tick.Tick++;
            worldTime.tick.TickDuration = worldTime.tick.TickInterval;
       //     worldTime.tick.FrameDuration = worldTime.tick.TickInterval;
            SetSingleton(worldTime);
            base.OnUpdate();
        }

        protected override void OnUpdate()
        {
            var worldTime = GetSingleton<WorldTime>();          
            int tickCount = 0;
            while (worldTime.frameTime > nextTickTime)
            {
                tickCount++;
                ServerTick();
              //  if (gameWorld.Tick % 10 == 0)
                //    Thread.Sleep(random.Next(30, 100));

                nextTickTime += worldTime.tick.TickInterval;
            }

            float remainTime = (float)(nextTickTime - worldTime.frameTime);

            int rate = worldTime.tick.TickRate;
            if (remainTime > 0.75f * worldTime.tick.TickInterval)
                rate -= 2;
            else if (remainTime < 0.25f * worldTime.tick.TickInterval)
                rate += 2;

            Application.targetFrameRate = rate;

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
