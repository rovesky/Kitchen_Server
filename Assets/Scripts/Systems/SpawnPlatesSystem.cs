using FootStone.ECS;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.ECS
{
    [DisableAutoCreation]
    public class SpawnPlatesSystem : ComponentSystem
    {
        private Entity platePrefab;
        private NetworkServerSystem networkServerSystem;
        private bool isSpawned = false;

        protected override void OnCreate()
        {
            base.OnCreate();

            platePrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(
            Resources.Load("Plate") as GameObject, World.Active);

            networkServerSystem = World.GetOrCreateSystem<NetworkServerSystem>();

        }

        protected override void OnUpdate()
        {
            if (isSpawned)
                return;

            isSpawned = true;

			var query = GetEntityQuery(typeof(TriggerDataComponent));

			var entities = query.ToEntityArray(Allocator.TempJob);

			for (int i = 0; i < 3; ++i)
			{
				var entity = entities[i * 2];
				var slot = EntityManager.GetComponentData<SlotComponent>(entity);
				var pos = EntityManager.GetComponentData<LocalToWorld>(slot.SlotEntity);

				var e = EntityManager.Instantiate(platePrefab);
				Translation position = new Translation() { Value = pos.Position };
				Rotation rotation = new Rotation() { Value = Quaternion.identity };

				EntityManager.SetComponentData(e, position);
				EntityManager.SetComponentData(e, rotation);

				var id = networkServerSystem.RegisterEntity(1, -1, e);
				EntityManager.AddComponentData(e, new Plate { id = id, IsFree = false });

				slot.FiltInEntity = e;
				EntityManager.SetComponentData(entity, slot);

				EntityManager.AddComponentData(e, new ItemInterpolatedState()
				{
					position = position.Value,
					rotation = Quaternion.identity,
					owner = Entity.Null
				});
			}
			entities.Dispose();

        }
    }
}