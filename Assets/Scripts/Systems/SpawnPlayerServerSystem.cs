using FootStone.ECS;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
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

        private float3 GetSpawnPoint()
        {
            var spawnPoints = spawnPointQuery.ToEntityArray(Allocator.TempJob);

            ref var physicsWorld = ref m_BuildPhysicsWorldSystem.PhysicsWorld;


            var position = float3.zero; 
            foreach (var spawnPoint in spawnPoints)
            {
                position = EntityManager.GetComponentData<LocalToWorld>(spawnPoint).Position;

                if(!IsOther(ref physicsWorld,position))
                    break;
             
            }
        
            spawnPoints.Dispose();
            return position;
        }

        private bool IsOther(ref PhysicsWorld physicsWorld,float3 position)
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

