using FootStone.ECS;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
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
                Resources.Load("Player1") as GameObject, World.Active);

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
           
                var position = new Translation {Value = {x = 0, y = 1, z = -5}};
                var rotation = new Rotation {Value = Quaternion.identity};

                EntityManager.SetComponentData(e, position);
                EntityManager.SetComponentData(e, rotation);

                EntityManager.AddComponentData(e, new ReplicatedEntityData()
                {
                    Id = -1,
                    PredictingPlayerId = spawnPlayer.PlayerId
                });

                EntityManager.AddComponentData(e, new Player());
                EntityManager.AddComponentData(e, new CharacterMove
                {
                    SkinWidth = 0.02f,
                    Velocity = 6.0f
                });


                EntityManager.AddComponentData(e, new UserCommand
                {
                    renderTick = 0,
                    targetPos = Vector3.zero
                });
                EntityManager.AddBuffer<UserCommandBuffer>(e);

                EntityManager.AddComponentData(e, new Connection
                {
                    id = spawnPlayer.PlayerId,
                    sessionId = spawnPlayer.PlayerId
                });

                EntityManager.AddComponentData(e, new CharacterInterpolatedState
                {
                    Position = position.Value,
                    Rotation = rotation.Value
                });

                EntityManager.AddComponentData(e, new CharacterPredictedState
                {
                    Position = position.Value,
                    Rotation = rotation.Value,
                    PickupedEntity = Entity.Null
                });

                EntityManager.AddComponentData(e, new PickupItem());

                EntityManager.AddComponentData(e, new ThrowItem
                {
                    speed = 14
                });

                var id = networkServerSystem.RegisterEntity(0, spawnPlayer.PlayerId, e);

                var replicatedEntityData = EntityManager.GetComponentData<ReplicatedEntityData>(e);
                replicatedEntityData.Id = id;
                EntityManager.SetComponentData(e, replicatedEntityData);

            }

            array.Dispose();
        }
    }
}