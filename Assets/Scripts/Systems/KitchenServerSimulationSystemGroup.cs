using System;
using FootStone.ECS;
using Unity.Entities;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

namespace FootStone.Kitchen
{
	//[UpdateAfter(typeof(ExportPhysicsWorld)), UpdateBefore(typeof(EndFramePhysicsSystem))]
    [UpdateBefore(typeof(TransformSystemGroup))]
	public class KitchenServerSimulationSystemGroup : NoSortComponentSystemGroup
    {

        private NetworkServerSystem networkServerSystem;
        private double nextTickTime;
        private long simStartTime;
        private uint simStartTimeTick;
        private float lastSimTime;

        protected override void OnCreate()
        {
            ConfigVar.Init();

            var commandLineArgs = Environment.GetCommandLineArgs();
            foreach (var commandLine in commandLineArgs)
            {
                if(commandLine.StartsWith("-Port:"))
                    NetworkConfig.serverPort.Value = commandLine.Substring(6);
                FSLog.Info($"commandLineArgs:{commandLine}");
            }
        

            ItemCreateUtilities.Init();

            GameWorld.Active = new GameWorld();
            UnityEngine.Time.fixedDeltaTime = GameTick.DefaultGameTick.TickInterval;

            EntityManager.CreateEntity(typeof(Server));
            SetSingleton(new Server());

            m_systemsToUpdate.Add(World.GetOrCreateSystem<WorldSceneEntitiesSystem>());
            networkServerSystem = World.GetOrCreateSystem<NetworkServerSystem>();

            m_systemsToUpdate.Add(World.GetOrCreateSystem<ReplicateEntityServerSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystem<HandleCommandsSystem>());

            m_systemsToUpdate.Add(World.GetOrCreateSystem<InitSystemGroup>());
            m_systemsToUpdate.Add(World.GetOrCreateSystem<SpawnPlayerServerSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystem<SpawnRobotsSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystem<SpawnSystemGroup>());

            m_systemsToUpdate.Add(World.GetOrCreateSystem<RegisterEntityToNetwork>());

            m_systemsToUpdate.Add(World.GetOrCreateSystem<PrintGameStateSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystem<RobotsControlSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystem<UpdateReplicatedOwnerFlag>());

            m_systemsToUpdate.Add(World.GetOrCreateSystem<PredictUpdateSystemGroup>());
            m_systemsToUpdate.Add(World.GetOrCreateSystem<ServerSystemGroup>());

            m_systemsToUpdate.Add(World.GetOrCreateSystem<PredictPresentationSystemGroup>());
            
            m_systemsToUpdate.Add(World.GetOrCreateSystem<DespawnServerSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystem<DespawnCharacterSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystem<DespawnItemOwnerSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystem<DespawnSystem>());            
        }


        private void ServerTick()
        {
            var worldTime = GetSingleton<WorldTime>();
            worldTime.GameTick.Tick++;
            worldTime.GameTick.TickDuration = worldTime.GameTick.TickInterval;
            //     worldTime.tick.FrameDuration = worldTime.tick.TickInterval;
            networkServerSystem.HandleClientCommands((int)worldTime.GameTick.Tick);
            SetSingleton(worldTime);
            base.OnUpdate();
        }

        protected override void OnUpdate()
        {
            var worldTime = GetSingleton<WorldTime>();

            simStartTime = GameWorld.Active.Clock.ElapsedTicks;
            simStartTimeTick = worldTime.Tick;

            networkServerSystem.Update();

            //   int tickCount = 0;
            while (worldTime.FrameTime > nextTickTime)
            {
                //    tickCount++;
                ServerTick();
                //  if (gameWorld.Tick % 10 == 0)
                //    Thread.Sleep(random.Next(30, 100));
                networkServerSystem.GenerateSnapshot(lastSimTime);
                nextTickTime += worldTime.GameTick.TickInterval;
            }

            var remainTime = (float) (nextTickTime - worldTime.FrameTime);

            var rate = worldTime.GameTick.TickRate;
            if (remainTime > 0.75f * worldTime.GameTick.TickInterval)
                rate -= 2;
            else if (remainTime < 0.25f * worldTime.GameTick.TickInterval)
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
