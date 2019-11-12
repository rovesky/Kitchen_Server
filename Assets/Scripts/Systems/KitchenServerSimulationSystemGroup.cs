﻿using FootStone.ECS;
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
    public class KitchenServerSimulationSystemGroup : ComponentSystemGroup
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
            m_systemsToUpdate.Add(World.GetOrCreateSystem<HandleCommandSystem>());

            m_systemsToUpdate.Add(World.GetOrCreateSystemE<SpawnPlayerServerSystem>());
            //   m_systemsToUpdate.Add(World.GetOrCreateSystemE<RayCastSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystemE<MoveInputSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystemE<ApplyPresentationSystem>());

            m_systemsToUpdate.Add(World.GetOrCreateSystemE<DespawnServerSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystemE<DespawnSystem>());            
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
}