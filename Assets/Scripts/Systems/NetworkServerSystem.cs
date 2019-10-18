using FootStone.ECS;
using FootStone.Kcp;
using Unity.Entities;

namespace Assets.Scripts.ECS
{
    [DisableAutoCreation]
    public class NetworkServerSystem : FSComponentSystem, INetworkCallbacks
    {
        private KcpServer kcpServer;
      
        public void OnConnect(KcpConnection connection)
        {
            FSLog.Info($"server connection created:{connection.Id}");                 

            var entity = GetSingletonEntity<SpawnPlayerServer>();
            var buffer = EntityManager.GetBuffer<SpawnPlayerBuffer>(entity);
            buffer.Add(new SpawnPlayerBuffer() { playerId = (int)connection.SessionId, connectionId = connection.Id});

            //接收到客户端的指令
            connection.Recv += (inSequence, data) =>
            {
               // FSLog.Debug($"[{inSequence}] server recv data");
                Entities.ForEach((ref Connection con,ref UserCommand command) =>
                {
                    if (connection.Id == con.id)
                    {
                        command.FromData(data);
                     //   command.isBack = true;
                    }
                });
            };
        }

        public void OnDisconnect(int connectionId)
        {
            FSLog.Info($"server connection destroyed:{connectionId}");
            //删除Player
            Entities.ForEach((Entity entity,ref Connection connection) =>
            {
                if (connectionId == connection.id)
                {
                    EntityManager.AddComponentData(entity,new Despawn() { Frame = 0 });
                }
            });        
        }

        protected override void OnCreate()
        {
            base.OnCreate();          

            kcpServer = new KcpServer(this, 1001);
            FSLog.Info($"server listening on 1001!");
        }

        protected override void OnDestroy()
        {
            FSLog.Info($"NetworkServerSystem OnDestroy!");
            kcpServer.Shutdown();
        }

        protected  override void OnUpdate()
        {
          //  FSLog.Info($"NetworkServerSystem OnUpdate!"); 
            kcpServer.Update();
        }

        public void SendData(int id,byte[] data)
        {
            //  FSLog.Info($"NetworkServerSystem SendData!");
            kcpServer.SendData(id, data, data.Length);
        }
    }
}
