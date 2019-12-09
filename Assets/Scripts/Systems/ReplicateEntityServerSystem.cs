using System;
using FootStone.ECS;
using Unity.Entities;

namespace FootStone.Kitchen
{
    [DisableAutoCreation]
    public class ReplicateEntityServerSystem : ComponentSystem,  ISnapshotGenerator
    {
        private NetworkServerSystem networkServerSystem;
        private ReplicatedEntityCollection replicatedEntityCollection;
        private bool isInit;

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
            ////TODO 临时在这里生成Table数据
            if (isInit)
                return;
            isInit = true;
          //  FSLog.Info($"RegisterEntity");
            var worldSceneEntitiesSystem = World.GetOrCreateSystem<WorldSceneEntitiesSystem>();
            networkServerSystem.ReserveSceneEntities(worldSceneEntitiesSystem.SceneEntities.Count);
            for (var i = 0; i < worldSceneEntitiesSystem.SceneEntities.Count; ++i)
            {
                //FSLog.Info($"RegisterEntity:{i},{(ushort)EntityType.Table}");
                var entity = worldSceneEntitiesSystem.SceneEntities[i];
                networkServerSystem.RegisterEntity(i, (ushort)EntityType.Table, -1, entity);
            }
        }

      

        public void GenerateEntitySnapshot(int entityId, ref NetworkWriter writer)
        {
            replicatedEntityCollection.GenerateEntitySnapshot(entityId, ref writer);
        }

        public string GenerateEntityName(int entityId)
        {
            return replicatedEntityCollection.GenerateName(entityId);
        }
        public bool HasEntity(Entity owner)
        {
            var replicatedData = EntityManager.GetComponentData<ReplicatedEntityData>(owner);
            return replicatedEntityCollection.GetEntity(replicatedData.Id) == owner;
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