using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.ECS
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

            foreach (var playerBuffer in array)
            {
             //   var playerBuffer = array[i];

                //创建Player
                var e = EntityManager.Instantiate(playerPrefab);

                var id = networkServerSystem.RegisterEntity(0, playerBuffer.PlayerId, e);

                var position = new Translation {Value = {x = 0, y = 1, z = -5}};
                var rotation = new Rotation {Value = Quaternion.identity};

                EntityManager.SetComponentData(e, position);
                EntityManager.SetComponentData(e, rotation);
                EntityManager.AddComponentData(e, new Player {playerId = playerBuffer.PlayerId, id = id});
                EntityManager.AddComponentData(e, new CharacterMove
                {
                    SkinWidth = 0.02f,
                    Velocity = 6.0f
                });


                //EntityManager.AddComponentData(e, new MoveInput
                //{
                //    Speed = 6
                //});

                EntityManager.AddComponentData(e, new UserCommand
                {
                    renderTick = 0,
                    targetPos = Vector3.zero
                });
                EntityManager.AddBuffer<UserCommandBuffer>(e);

                EntityManager.AddComponentData(e, new Connection
                {
                    id = playerBuffer.PlayerId,
                    sessionId = playerBuffer.PlayerId
                });

                EntityManager.AddComponentData(e, new CharacterInterpolateState
                {
                    Position = position.Value,
                    Rotation = rotation.Value
                });

                EntityManager.AddComponentData(e, new CharacterPredictState
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
            }

            array.Dispose();
        }
    }
}