﻿using FootStone.ECS;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace FootStone.Kitchen
{
    [DisableAutoCreation]
    public class SpawnPlayerServerSystem : ComponentSystem
    {
        private NetworkServerSystem networkServerSystem;

        private Entity playerPrefab;
      

        protected override void OnCreate()
        {
            var entity = EntityManager.CreateEntity(typeof(SpawnPlayerServer));
            SetSingleton(new SpawnPlayerServer());
            EntityManager.AddBuffer<SpawnPlayerBuffer>(entity);
       
            playerPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(
                Resources.Load("Player1") as GameObject,
                GameObjectConversionSettings.FromWorld(World, 
                    World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<ConvertToEntitySystem>().BlobAssetStore));

            networkServerSystem = World.GetOrCreateSystem<NetworkServerSystem>();
        }

      

        protected override void OnUpdate()
        {
            var entity = GetSingletonEntity<SpawnPlayerServer>();
            var buffer = EntityManager.GetBuffer<SpawnPlayerBuffer>(entity);
            if (buffer.Length == 0)
                return;

            var array = buffer.ToNativeArray(Allocator.Temp);
            buffer.Clear();

            foreach (var spawnPlayer in array)
            {
                //创建Player
                var e = EntityManager.Instantiate(playerPrefab);

              //  var position = new Vector3 {x = 0, y = 1, z = -5};
                var rotation = Quaternion.identity;

                CreateCharacterUtilities.CreateCharacterComponent(EntityManager, e, spawnPlayer.Position, rotation);

                EntityManager.AddBuffer<UserCommandBuffer>(e);

                if (spawnPlayer.IsRobot)
                    EntityManager.AddComponentData(e, new Robot());
                else
                    EntityManager.AddComponentData(e, new Connection
                    {
                        SessionId = spawnPlayer.PlayerId
                    });
                  

                var id = networkServerSystem.RegisterEntity(-1, (ushort) EntityType.Character, spawnPlayer.PlayerId, e);

                var interpolatedState = EntityManager.GetComponentData<CharacterInterpolatedState>(e);
                interpolatedState.MaterialId =  (byte) (spawnPlayer.PlayerId % 4);
                EntityManager.SetComponentData(e, interpolatedState);

                EntityManager.SetComponentData(e, new ReplicatedEntityData
                {
                    Id = id,
                    PredictingPlayerId = spawnPlayer.PlayerId
                });
            }

            array.Dispose();
        }
    }
}

