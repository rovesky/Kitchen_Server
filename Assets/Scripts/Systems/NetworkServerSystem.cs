using FootStone.ECS;
using FootStone.Kcp;
using Unity.Entities;

namespace Assets.Scripts.ECS
{
    [DisableAutoCreation]
    public class NetworkServerSystem : FSComponentSystem, FootStone.Kcp.INetworkCallbacks
    {
        private KcpServer kcpServer;
        private HandleCommandSystem handleCommandSystem;

        public void OnConnect(KcpConnection connection)
        {
            FSLog.Info($"server connection created:{connection.Id}");                 

            var entity = GetSingletonEntity<SpawnPlayerServer>();
            var buffer = EntityManager.GetBuffer<SpawnPlayerBuffer>(entity);
            buffer.Add(new SpawnPlayerBuffer() { playerId = (int)connection.SessionId, connectionId = connection.Id});

            //接收到客户端的指令
            connection.Recv += (inSequence, data) =>
            {
                var tick = GetSingleton<WorldTime>().Tick;
             //  FSLog.Info($"ServerTick:{tick}");
                // FSLog.Debug($"[{inSequence}] server recv data");
                Entities.ForEach((Entity e,ref Connection con/*,ref UserCommand command*/) =>
                {
                    if (connection.Id == con.id)
                    {                      
                        var commandBuffer = new UserCommandBuffer();
                        commandBuffer.command.FromData(data);
                        FSLog.Info($"recv command:{commandBuffer.command.renderTick}," +
                            $"{commandBuffer.command.checkTick}," +
                            $"{tick}");
                            //$"{commandBuffer.command.buttons.flags},"+
                            //$"{commandBuffer.command.targetPos.x},"+
                            //$"{commandBuffer.command.targetPos.y},"+
                            //$"{commandBuffer.command.targetPos.z}");

                        if (commandBuffer.command.checkTick >= tick)
                        {
                            EntityManager.GetBuffer<UserCommandBuffer>(e).Add(commandBuffer);
                            FSLog.Info($"buffer command:{commandBuffer.command.renderTick},{commandBuffer.command.checkTick},{tick}");
                            //     FSLog.Info($"UserCommandBuffer add new {commandBuffer.command.checkTick},{tick}");
                        }
                    
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

        //    handleCommandSystem = World.GetOrCreateSystem<HandleCommandSystem>();
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
