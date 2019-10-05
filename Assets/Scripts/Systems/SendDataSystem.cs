using System.IO;
using Unity.Entities;

namespace Assets.Scripts.ECS
{

    [DisableAutoCreation]
    public class SendDataSystem : ComponentSystem
    {    
        private EntityQuery snapShotQuery;
        private NetworkServerSystem networkServer;

        protected override void OnCreate()
        {
            base.OnCreate();          
            snapShotQuery = GetEntityQuery(ComponentType.ReadOnly<Snapshot>());
            networkServer = World.GetExistingSystem<NetworkServerSystem>();
        }

        protected override void OnDestroy()
        {
  
        }

        protected  unsafe override void OnUpdate()
        {  
            //发送snapshot
            if (snapShotQuery.CalculateEntityCount() > 0)
            {
                var snapShotEntity = snapShotQuery.GetSingletonEntity();
                var buffer = EntityManager.GetBuffer<SnapshotTick>(snapShotEntity);
                if (buffer.Length == 0)
                    return;

                var snapshotTick = buffer[buffer.Length - 1];
                var length = snapshotTick.length;
                var data = new byte[length];
                using (UnmanagedMemoryStream tempUMS = new UnmanagedMemoryStream((byte*)snapshotTick.data, length))
                {
                    tempUMS.Read(data, 0, data.Length);
                }
              //   FSLog.Info($"snapshot data {data.Length}!");

                Entities.ForEach((ref Connection connection) =>
                {
                    networkServer.SendData(connection.id,data);                             
                });
              
            }
        }
    }
}
