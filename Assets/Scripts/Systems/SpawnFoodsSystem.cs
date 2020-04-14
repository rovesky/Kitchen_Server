using System.Collections.Generic;
using FootStone.ECS;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace FootStone.Kitchen
{
    [DisableAutoCreation]
    public class SpawnFoodsSystem : ComponentSystem
    {
        private NetworkServerSystem networkServerSystem;


        private Dictionary<EntityType,Entity> prefabs = new Dictionary<EntityType, Entity>();

        private void RegisterPrefabs(EntityType type, string res)
        {
            prefabs[type] = GameObjectConversionUtility.ConvertGameObjectHierarchy(
                Resources.Load(res) as GameObject,
                GameObjectConversionSettings.FromWorld(World,
                    World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<ConvertToEntitySystem>().BlobAssetStore));

        }

        protected override void OnCreate()
        {
            base.OnCreate();

            var entity = EntityManager.CreateEntity(typeof(SpawnFoodArray));
            SetSingleton(new SpawnFoodArray());
            EntityManager.AddBuffer<SpawnFoodRequest>(entity);

            RegisterPrefabs(EntityType.Shrimp, "Shrimp");
            RegisterPrefabs(EntityType.ShrimpSlice, "ShrimpSlice");
            RegisterPrefabs(EntityType.KelpSlice, "KelpSlice");
            RegisterPrefabs(EntityType.Rice, "Rice");
            RegisterPrefabs(EntityType.Cucumber, "Cucumber");
            RegisterPrefabs(EntityType.CucumberSlice, "CucumberSlice");

            networkServerSystem = World.GetOrCreateSystem<NetworkServerSystem>();

        }

        protected override void OnUpdate()
        {
          
            var entity = GetSingletonEntity<SpawnFoodArray>();
            var buffer = EntityManager.GetBuffer<SpawnFoodRequest>(entity);
            if (buffer.Length == 0)
                return;

            var array = buffer.ToNativeArray(Allocator.Temp);
            buffer.Clear();

            foreach (var spawnFood in array)
            {
                if(!prefabs.ContainsKey(spawnFood.Type))
                    continue;

                FSLog.Info($"spawnFood:{spawnFood.Type}");

                var prefab = prefabs[spawnFood.Type];
                var e = EntityManager.Instantiate(prefab);

                ItemCreateUtilities.CreateItemComponent(EntityManager, e,
                    spawnFood.Pos, quaternion.identity);
                EntityManager.AddComponentData(e, new Food()
                {
                    Type = spawnFood.Type
                });

                if(spawnFood.IsSlice)
                    EntityManager.AddComponentData(e, new Slice());

                var id = networkServerSystem.RegisterEntity(-1, (ushort) spawnFood.Type,
                    -1, e);

                EntityManager.SetComponentData(e, new ReplicatedEntityData
                {
                    Id = id,
                    PredictingPlayerId = -1
                });

                if (spawnFood.Owner == Entity.Null) 
                    continue;
              

                if (EntityManager.HasComponent<Character>(spawnFood.Owner))
                {
                    ItemAttachUtilities.ItemAttachToCharacter(EntityManager,e,spawnFood.Owner,-1);
                    var pickupState = EntityManager.GetComponentData<PickupPredictedState>(spawnFood.Owner);
                    pickupState.PickupedEntity = e;
                    EntityManager.SetComponentData(spawnFood.Owner,pickupState);
                }
                else if (EntityManager.HasComponent<SlotPredictedState>(spawnFood.Owner))
                {
                    ItemAttachUtilities.ItemAttachToTable(EntityManager,e,
                        spawnFood.Owner,spawnFood.Pos);
                    EntityManager.SetComponentData(spawnFood.Owner, new SlotPredictedState()
                    {
                        FilledInEntity = e
                    });
                }
            }

            array.Dispose();
        }
    }
}