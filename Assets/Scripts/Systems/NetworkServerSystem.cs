using FootStone.ECS;
using Unity.Entities;
using Unity.Mathematics;

namespace FootStone.Kitchen
{
    [DisableAutoCreation]
    public class NetworkServerSystem : ComponentSystem, INetworkCallbacks, /*ISnapshotGenerator,*/ IClientCommandProcessor
    {
        private HandleCommandsSystem handleCommandSystem;

        private NetworkServer network;
        private ReplicateEntityServerSystem replicateEntityServerSystem;

     
        public void ProcessCommand(int connectionId, int tick, ref NetworkReader data)
        {
            var serverTick = GetSingleton<WorldTime>().Tick;
            var commandBuffer = new UserCommandBuffer();
            commandBuffer.Command.Deserialize(ref data);
            //  FSLog.Info($"ServerTick:{tick}");
            // FSLog.Debug($"[{inSequence}] server recv data");
            Entities.ForEach((Entity e, ref Connection con /*,ref UserCommand command*/) =>
            {
                //FSLog.Info($"[{ConnectionId},{con.id}] recv command:{commandBuffer.command.renderTick}," +
                //      $"{commandBuffer.command.checkTick}," +
                //      $"{tick}");
                if (connectionId != con.SessionId)
                    return;

                if (commandBuffer.Command.CheckTick >= serverTick)
                    EntityManager.GetBuffer<UserCommandBuffer>(e).Add(commandBuffer);
                //   FSLog.Info($"buffer command:{commandBuffer.command.renderTick},{commandBuffer.command.checkTick},{tick}");
                //     FSLog.Info($"UserCommandBuffer add new {commandBuffer.command.checkTick},{tick}");
            });
        }

        public void OnConnect(int clientId)
        {
            FSLog.Info($"client {clientId} connected!");

            var entity = GetSingletonEntity<SpawnPlayerServer>();
            var buffer = EntityManager.GetBuffer<SpawnPlayerBuffer>(entity);
            buffer.Add(new SpawnPlayerBuffer
            {
                PlayerId = clientId,
                IsRobot = false,
                Position = new float3(0, 1,  -5)
            });

            network.MapReady(clientId);
        }

        public void OnDisconnect(int clientId)
        {
            //throw new System.NotImplementedException();
            FSLog.Info($"client {clientId} Disconnected!");

            Entities.ForEach((Entity entity, ref Connection connection) =>
            {
                if (clientId == connection.SessionId)
                    EntityManager.AddComponentData(entity, new Despawn {Tick = 1});
            });
        }

        public void OnEvent(int clientId, NetworkEvent info)
        {
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            network = new NetworkServer(GameWorld.Active);
            network.InitializeMap((ref NetworkWriter data) => { data.WriteString("name", "plane"); });

            replicateEntityServerSystem = World.GetOrCreateSystem<ReplicateEntityServerSystem>();
       
            FSLog.Info("server listening on 1001!");
        }

        protected override void OnDestroy()
        {
            FSLog.Info("NetworkServerSystem OnDestroy!");

            network.Shutdown();
        }

        protected override void OnUpdate()
        {
            network.Update(this);
        }

        public void SendData()
        {
            network.SendData();
        }

        public void GenerateSnapshot(float lastSimTime)
        {
            network.GenerateSnapshot(replicateEntityServerSystem, lastSimTime);
        }
   

        public int RegisterEntity(int inId,ushort typeId, int predictingClientId,Entity entity)
        {
            var id =  network.RegisterEntity(inId, typeId, predictingClientId);
            replicateEntityServerSystem.RegisterEntity(id, entity);
            return id;
        }

        public void UnRegisterEntity(int id)
        {
            network.UnregisterEntity(id);
            replicateEntityServerSystem.UnRegisterEntity(id);
        }

        public void ReserveSceneEntities(int count)
        {
            network.ReserveSceneEntities(count);
        }

        public void HandleClientCommands(int tick)
        {
            network.HandleClientCommands(tick, this);
        }
    }
}