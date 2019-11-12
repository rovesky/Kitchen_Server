using System;
using System.Collections.Generic;
using FootStone.ECS;
using Unity.Entities;

namespace Assets.Scripts.ECS
{
    [DisableAutoCreation]
    public class NetworkServerNewSystem : FSComponentSystem, INetworkCallbacks, ISnapshotGenerator,IClientCommandProcessor
    {

        private HandleCommandSystem handleCommandSystem;

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
                writer.WriteInt32("id",player.id);
                writer.WriteInt32("playerId",player.playerId);

                var entityPredictData = EntityManager.GetComponentData<EntityPredictData>(entity);
                writer.WriteVector3Q("pos", entityPredictData.position);
                writer.WriteQuaternionQ("rotation", entityPredictData.rotation);

                var health = EntityManager.GetComponentData<Health>(entity);
                writer.WriteInt32("health",health.Value);

                var score = EntityManager.GetComponentData<Score>(entity);
                writer.WriteInt32("score",score.ScoreValue);
                writer.WriteInt32("maxScore", score.MaxScoreValue);
            }
            else if (EntityManager.HasComponent<Enemy>(entity))
            {
                var enemy = EntityManager.GetComponentData<Enemy>(entity);
                writer.WriteInt32("id",enemy.id);
                writer.WriteByte("type",(byte)enemy.type);

                var pos = EntityManager.GetComponentData<EntityPredictData>(entity).position;
                writer.WriteVector3Q("pos", pos);    

                var health = EntityManager.GetComponentData<Health>(entity);
                writer.WriteInt32("health",health.Value);

                var attack = EntityManager.GetComponentData<Attack>(entity);
                writer.WriteInt32("attack",attack.Power);

                if (enemy.type == EnemyType.Super)
                {
                    var fireRocket = EntityManager.GetComponentData<FireRocket>(entity);
                    writer.WriteFloatQ("fireCooldown",fireRocket.FireCooldown);
                    writer.WriteFloatQ("rocketTimer", fireRocket.RocketTimer);
                }
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
