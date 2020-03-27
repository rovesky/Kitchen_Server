using FootStone.ECS;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace FootStone.Kitchen
{
    [DisableAutoCreation]
    public class SpawnPlatesSystem : ComponentSystem
    {
        private bool isSpawned;
        private NetworkServerSystem networkServerSystem;
        private Entity platePrefab;

        protected override void OnCreate()
        {
            base.OnCreate();

            platePrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(
                Resources.Load("Apple") as GameObject,
                GameObjectConversionSettings.FromWorld(World,
                    World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<ConvertToEntitySystem>().BlobAssetStore));

            networkServerSystem = World.GetOrCreateSystem<NetworkServerSystem>();

        }

        protected override void OnUpdate()
        {
            if (isSpawned)
                return;

            isSpawned = true;

            var query = GetEntityQuery(typeof(TriggeredSetting));
            var entities = query.ToEntityArray(Allocator.TempJob);
          

            //生成Plate
            for (var i = 0; i < 3; ++i)
            {
                var entity = entities[i * 2];
                var slot = EntityManager.GetComponentData<SlotPredictedState>(entity);
                var triggerData = EntityManager.GetComponentData<TriggeredSetting>(entity);
                //  var pos = EntityManager.GetComponentData<LocalToWorld>(slot.SlotPos);

                var e = EntityManager.Instantiate(platePrefab);

                slot.FilledInEntity = e;
                EntityManager.SetComponentData(entity, slot);

                CreateItemUtilities.CreateItemComponent(EntityManager, e,
                    triggerData.SlotPos, quaternion.identity);

                var id = networkServerSystem.RegisterEntity(-1, (ushort)EntityType.Plate, -1, e);
                var replicatedEntityData = EntityManager.GetComponentData<ReplicatedEntityData>(e);
                replicatedEntityData.Id = id;
                replicatedEntityData.PredictingPlayerId = -1;
                EntityManager.SetComponentData(e, replicatedEntityData);
            }

            entities.Dispose();
        }
    }
}