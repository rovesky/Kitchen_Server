using FootStone.Kcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

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
                EntityManager.AddBuffer<PlayerId>(entity);
                var buffer =  EntityManager.GetBuffer<PlayerId>(entity);
                buffer.Add(new PlayerId() { playerId = connection.Id });
                spawn.spawned = true;
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
         //   snapShotQuery = GetEntityQuery(ComponentType.ReadOnly<Snapshot>());
            kcpServer = new KcpServer(this, 1001);
            FSLog.Info($"server listening on 1001!");
        }

        protected override void OnDestroy()
        {
            connections.Clear();
            if (kcpServer != null)
                kcpServer.Shutdown();
        }
        protected override void OnUpdate()
        {
            if (kcpServer == null)
                return;

            kcpServer.Update();
        //    FSLog.Info($"kcpServer.Update()!");
            return;
            //发送snapshot
            //if (snapShotQuery.CalculateEntityCount() > 0)
            //{
            //    var snapShotEntity = snapShotQuery.GetSingletonEntity();
            //    var buffer = EntityManager.GetBuffer<SnapshotTick>(snapShotEntity);
            //    var data = buffer.Last().data.ToArray();
            //    foreach (var connectionId in connections)
            //    {                   
            //        kcpServer.SendData(connectionId, data, data.Length);
            //    }
            //}
        }

    }
}
