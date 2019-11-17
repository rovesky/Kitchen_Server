using System;
using System.Collections.Generic;
using FootStone.ECS;
using Unity.Entities;
using Unity.Transforms;

namespace Assets.Scripts.ECS
{
    [DisableAutoCreation]
    public class NetworkServerSystem : ComponentSystem, INetworkCallbacks, ISnapshotGenerator,IClientCommandProcessor
    {

        private HandleCommandsSystem handleCommandSystem;

        private Dictionary<int, Entity> entities = new Dictionary<int, Entity>();

        private NetworkServer network;

        public int WorldTick => (int)GetSingleton<WorldTime>().Tick;

        protected override void OnCreate()
        {
            base.OnCreate();

            network = new NetworkServer();

            network.InitializeMap((ref NetworkWriter data) =>
            {
                data.WriteString("name", "plane");
            });

            FSLog.Info($"server listening on 1001!");
        }

        protected override void OnDestroy()
        {
            FSLog.Info($"NetworkServerSystem OnDestroy!");

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

        public void OnConnect(int clientId)
        {
            FSLog.Info($"client {clientId} connected!");

            var entity = GetSingletonEntity<SpawnPlayerServer>();
            var buffer = EntityManager.GetBuffer<SpawnPlayerBuffer>(entity);
            buffer.Add(new SpawnPlayerBuffer() { playerId = clientId });

            network.MapReady(clientId);
        }

        public void OnDisconnect(int clientId)
        {
            //throw new System.NotImplementedException();
            FSLog.Info($"client {clientId} Disconnected!");

            Entities.ForEach((Entity entity, ref Connection connection) =>
            {
                if (clientId == connection.sessionId)
                {
                    EntityManager.AddComponentData(entity, new Despawn() { Frame = 0 });
                }
            });
        }

        public void OnEvent(int clientId, NetworkEvent info)
        {


        }
       

        public void GenerateSnapshot(float lastSimTime)
        {
            network.GenerateSnapshot(this, lastSimTime);
        }


        public int RegisterEntity(ushort typeId, int predictingClientId, Entity entity)
        {
            var id = network.RegisterEntity(-1, typeId, predictingClientId);
            entities[id] = entity;
            return id;
        }

        public void UnRegisterEntity(int id)
        {
            network.UnregisterEntity(id);
            entities.Remove(id);
        }

        public void GenerateEntitySnapshot(int entityId, ref NetworkWriter writer)
        {
            var entity = entities[entityId];

            if (EntityManager.HasComponent<Player>(entity))
            {
                var player = EntityManager.GetComponentData<Player>(entity);
                writer.WriteInt32("id", player.id);
                writer.WriteInt32("playerId", player.playerId);

                var entityPredictData = EntityManager.GetComponentData<CharacterPredictState>(entity);
                writer.WriteVector3Q("pos", entityPredictData.position);
                writer.WriteQuaternionQ("rotation", entityPredictData.rotation);

                int id = 0;
                if (entityPredictData.pickupEntity == Entity.Null || !EntityManager.HasComponent<Plate>(entityPredictData.pickupEntity))
                    id = -1;
                else
                    id = EntityManager.GetComponentData<Plate>(entityPredictData.pickupEntity).id;
                writer.WriteInt32("pickupEntity", id);

            }
            else if (EntityManager.HasComponent<Plate>(entity))
            {
                //FSLog.Info($"GenerateEntitySnapshot Plate:{entityId}");
                var plate = EntityManager.GetComponentData<Plate>(entity);
                writer.WriteInt32("id", plate.id);

                var itemState = EntityManager.GetComponentData<ItemInterpolatedState>(entity);
                writer.WriteVector3Q("pos", itemState.position);
                writer.WriteQuaternionQ("rotation", itemState.rotation);

                int id = 0;
                if (itemState.owner == Entity.Null || !EntityManager.HasComponent<Player>(itemState.owner))
                    id = -1;
                else
                    id = EntityManager.GetComponentData<Player>(itemState.owner).id;
                writer.WriteInt32("owner", id);
             
            }
        }

        public void HandleClientCommands(int tick)
        {
            network.HandleClientCommands(tick, this);
        }

        public string GenerateEntityName(int entityId)
        {
            return entityId.ToString();
        }

        public void ProcessCommand(int connectionId, int tick, ref NetworkReader data)
        {
            var serverTick = GetSingleton<WorldTime>().Tick;
            var commandBuffer = new UserCommandBuffer();
            commandBuffer.command.Deserialize(ref data);
            //  FSLog.Info($"ServerTick:{tick}");
            // FSLog.Debug($"[{inSequence}] server recv data");
            Entities.ForEach((Entity e, ref Connection con/*,ref UserCommand command*/) =>
            {
                //FSLog.Info($"[{connectionId},{con.id}] recv command:{commandBuffer.command.renderTick}," +
                //      $"{commandBuffer.command.checkTick}," +
                //      $"{tick}");
                if (connectionId == con.id)
                {  
                    if (commandBuffer.command.checkTick >= serverTick)
                    {
                        EntityManager.GetBuffer<UserCommandBuffer>(e).Add(commandBuffer);
                     //   FSLog.Info($"buffer command:{commandBuffer.command.renderTick},{commandBuffer.command.checkTick},{tick}");
                        //     FSLog.Info($"UserCommandBuffer add new {commandBuffer.command.checkTick},{tick}");
                    }
                }
            });
        }
    }
}
