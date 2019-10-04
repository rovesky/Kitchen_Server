using FootStone.Kcp;
using System.Collections.Generic;
using Unity.Entities;

namespace Assets.Scripts.ECS
{
    [DisableAutoCreation]
    public class NetworkServerSystem : ComponentSystem, INetworkCallbacks
    {
        private KcpServer kcpServer;
        private EntityQuery snapShotQuery;

        private List<int> connections = new List<int>();

        public void OnConnect(KcpConnection connection)
        {
            FSLog.Info($"server connection created:{connection.Id}");
            connections.Add(connection.Id);

            Entities.ForEach((Entity entity,ref SpawnPlayer spawn) =>
            {
                FSLog.Info($"SpawnPlayer:{connection.Id}");
                EntityManager.AddBuffer<PlayerBuffer>(entity);
                var buffer =  EntityManager.GetBuffer<PlayerBuffer>(entity);
                buffer.Add(new PlayerBuffer() { playerId = connection.Id });
                spawn.spawn = true;
            });

          
            connection.Recv += (inSequence, buffer) =>
            {
                FSLog.Debug($"[{inSequence}] server recv data");
                Entities.ForEach((Entity entity, ref PlayerCommand command) =>
                {
                    command.FromData(buffer);
                    command.isBack = true;
                });
            };
        }

        public void OnDisconnect(int connectionId)
        {
            FSLog.Info($"server connection destroyed:{connectionId}");
            connections.Remove(connectionId);
        }

        protected override void OnCreate()
        {
            base.OnCreate();          

            kcpServer = new KcpServer(this, 1001);
            FSLog.Info($"server listening on 1001!");
        }

        protected override void OnDestroy()
        {
            connections.Clear();
            if (kcpServer != null)
                kcpServer.Shutdown();
        }

        protected  unsafe override void OnUpdate()
        {
            if (kcpServer == null)
                return;

            kcpServer.Update();
        }

        public void SendData(byte[] data)
        {
            foreach (var connectionId in connections)
            {
                kcpServer.SendData(connectionId, data, data.Length);
            }
        }
    }
}
