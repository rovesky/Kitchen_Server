using FootStone.ECS;
using Unity.Entities;

namespace FootStone.Kitchen
{
    [DisableAutoCreation]
    public class ReplicateEntityServerSystem : ComponentSystem,  ISnapshotGenerator
    {
        private NetworkServerSystem networkServerSystem;
        private ReplicatedEntityCollection replicatedEntityCollection;

        public int WorldTick => (int) GetSingleton<WorldTime>().Tick;
      

        protected override void OnCreate()
        {
            networkServerSystem = World.GetOrCreateSystem<NetworkServerSystem>();
            replicatedEntityCollection = new ReplicatedEntityCollection(EntityManager);
        }

        protected override void OnDestroy()
        {
            FSLog.Info("ReplicateEntityServerSystem OnDestroy!");
          
        }

        protected override void OnUpdate()
        {
           
        }

        public void GenerateEntitySnapshot(int entityId, ref NetworkWriter writer)
        {
            replicatedEntityCollection.GenerateEntitySnapshot(entityId, ref writer);
        }

        public string GenerateEntityName(int entityId)
        {
            return replicatedEntityCollection.GenerateName(entityId);
        }

        public void RegisterEntity(int id, Entity entity){
          
            replicatedEntityCollection.Register(id, entity);
        }

        public void UnRegisterEntity(int id)
        {
            replicatedEntityCollection.Unregister(id);
        }
     
    }
}