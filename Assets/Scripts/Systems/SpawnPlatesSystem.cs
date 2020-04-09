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
                Resources.Load("Plate") as GameObject,
                GameObjectConversionSettings.FromWorld(World,
                    World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<ConvertToEntitySystem>().BlobAssetStore));

            networkServerSystem = World.GetOrCreateSystem<NetworkServerSystem>();

        }

        protected override void OnUpdate()
        {
            if (isSpawned)
                return;

            isSpawned = true;

            var query = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(Table)
                },
                None = new ComponentType[]
                {
                    typeof(BoxSetting),
                    typeof(TableSlice)
                }
            });
            var entities = query.ToEntityArray(Allocator.TempJob);
          

            //生成Plate
            for (var i = 0; i < 3; ++i)
            {
                var entity = entities[i * 2];
                var slot = EntityManager.GetComponentData<SlotPredictedState>(entity);
                var slotData = EntityManager.GetComponentData<SlotSetting>(entity);

                //  var pos = EntityManager.GetComponentData<LocalToWorld>(slot.SlotPos);

                var e = EntityManager.Instantiate(platePrefab);
                slot.FilledInEntity = e;
                EntityManager.SetComponentData(entity, slot);

                ItemCreateUtilities.CreateItemComponent(EntityManager, e,
                    slotData.Pos, quaternion.identity);

               // FSLog.Info($"ItemPredictedState,Owner:{entity}");
                EntityManager.SetComponentData(e,new ItemPredictedState()
                {
                    Owner = entity,
                    PreOwner = Entity.Null
                });

                var itemPredictedState = EntityManager.GetComponentData<ItemPredictedState>(e);

                FSLog.Info($"ItemPredictedState,Owner:{itemPredictedState.Owner}");
                EntityManager.AddComponentData(e, new Plate());
                EntityManager.AddComponentData(e, new PlatePredictedState()
                {
                    Material1 = Entity.Null,
                    Material2 = Entity.Null,
                    Material3 = Entity.Null,
                    Material4 = Entity.Null
                });

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