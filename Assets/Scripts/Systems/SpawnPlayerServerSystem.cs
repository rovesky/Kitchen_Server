using System;
using System.Collections.Generic;
using FootStone.ECS;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace FootStone.Kitchen
{
    [DisableAutoCreation]
    public class SpawnPlayerServerSystem : ComponentSystem
    {
        private NetworkServerSystem networkServerSystem;
        private EntityQuery spawnPointQuery;
        private Entity playerPrefab;
        private KitchenBuildPhysicsWorld m_BuildPhysicsWorldSystem;
        private Unity.Mathematics.Random random;

        protected override void OnCreate()
        {
            var entity = EntityManager.CreateEntity(typeof(SpawnPlayerServer));
            SetSingleton(new SpawnPlayerServer());
            EntityManager.AddBuffer<SpawnPlayerBuffer>(entity);

            playerPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(
                Resources.Load("Character/CharacterEntity") as GameObject,
                GameObjectConversionSettings.FromWorld(World,
                    World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<ConvertToEntitySystem>().BlobAssetStore));

            networkServerSystem = World.GetOrCreateSystem<NetworkServerSystem>();

            spawnPointQuery = EntityManager.CreateEntityQuery(typeof(SpawnPoint));

            m_BuildPhysicsWorldSystem = World.GetOrCreateSystem<KitchenBuildPhysicsWorld>();

            random = new Unity.Mathematics.Random(20);

        }



        protected override void OnUpdate()
        {

            var entity = GetSingletonEntity<SpawnPlayerServer>();
            var buffer = EntityManager.GetBuffer<SpawnPlayerBuffer>(entity);
            if (buffer.Length == 0)
                return;

            var spawnPlayers = buffer.ToNativeArray(Allocator.Temp);
            buffer.Clear();
            
            foreach (var spawnPlayer in spawnPlayers)
            {
                //创建Player
                var e = EntityManager.Instantiate(playerPrefab);

                var position = GetSpawnPoint();
                var rotation = Quaternion.identity;

                CreateCharacterUtilities.CreateCharacterComponent(EntityManager, e, position, rotation);

                EntityManager.AddBuffer<UserCommandBuffer>(e);

                if (spawnPlayer.IsRobot)
                    EntityManager.AddComponentData(e, new Robot());
                else
                    EntityManager.AddComponentData(e, new Connection
                    {
                        SessionId = spawnPlayer.PlayerId
                    });
                  

                var id = networkServerSystem.RegisterEntity(-1, (ushort) EntityType.Character, spawnPlayer.PlayerId, e);

                //var interpolatedState = EntityManager.GetComponentData<CharacterInterpolatedState>(e);
                //interpolatedState.MaterialId =  (byte) (spawnPlayer.PlayerId % 4);
                //EntityManager.SetComponentData(e, interpolatedState);

                EntityManager.SetComponentData(e, new ReplicatedEntityData
                {
                    Id = id,
                    PredictingPlayerId = spawnPlayer.PlayerId
                });
            }

           
            spawnPlayers.Dispose();
        }

        /// <summary>
        /// 获取出生点
        /// </summary>
        /// <returns></returns>
        private float3 GetSpawnPoint()
        {
            var spawnPoints = spawnPointQuery.ToEntityArray(Allocator.TempJob);

            if(spawnPoints.Length == 0)
                return float3.zero;

            ref var physicsWorld = ref m_BuildPhysicsWorldSystem.PhysicsWorld;
            
            //找出所有没有被占位的出生点
            var validSpawnPoints = new  List<float3>();
            foreach (var spawnPoint in spawnPoints)
            {
                var position = EntityManager.GetComponentData<LocalToWorld>(spawnPoint).Position;

                if (!HasCharacter(ref physicsWorld, position))
                {
                    validSpawnPoints.Add(position);
                }
            }
        
            spawnPoints.Dispose();

            //获取随机出生点
            if(validSpawnPoints.Count == 0)
                return  EntityManager.GetComponentData<LocalToWorld>(spawnPoints[0]).Position;;
            return validSpawnPoints[random.NextInt(0,validSpawnPoints.Count)];
        }

        private bool HasCharacter(ref PhysicsWorld physicsWorld,float3 position)
        {
            var distanceHits = new NativeList<DistanceHit>(Allocator.Temp);

            // Character transform
            var input = new PointDistanceInput
            {
                MaxDistance = 0.1f,
                Position = position,
                Filter = CollisionFilter.Default
            };
              
            physicsWorld.CalculateDistance(input, ref distanceHits);
         
            foreach (var hit in distanceHits)
            {
                var e = physicsWorld.Bodies[hit.RigidBodyIndex].Entity;
                if (!EntityManager.HasComponent<Character>(e))
                    continue;
                distanceHits.Dispose();
                return true;
            }

            distanceHits.Dispose();

            return false ;
        }
    }
}

