using FootStone.ECS;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.ECS
{
    [DisableAutoCreation]
    public class SpawnPlatesSystem : ComponentSystem
    {
        private Entity platePrefab;
        private NetworkServerNewSystem networkServerSystem;
        private bool isSpawned = false;

        protected override void OnCreate()
        {
            base.OnCreate();

            platePrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(
            Resources.Load("Plate") as GameObject, World.Active);

            networkServerSystem = World.GetOrCreateSystem<NetworkServerNewSystem>();

        }

        protected override void OnUpdate()
        {
            if (isSpawned)
                return;

            isSpawned = true;

            var e = EntityManager.Instantiate(platePrefab);

            var id = networkServerSystem.RegisterEntity(1, -1, e);
            EntityManager.AddComponentData(e, new Plate() { id = id });

            var position = new Translation() { Value = { x = -4, y = 1, z = -1 } };

            EntityManager.SetComponentData(e, position);
            //EntityManager.AddComponentData(e, new ItemState()
            //{
            //    position = position.Value,
            //    rotation = Quaternion.identity,
            //    owner = Entity.Null
            //});

            EntityManager.AddComponentData(e, new ItemInterpolatedState()
            {
                position = position.Value,
                rotation = Quaternion.identity,
                owner = Entity.Null
            });

        }
    }
}