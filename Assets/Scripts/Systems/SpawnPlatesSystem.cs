using System.Linq;
using FootStone.ECS;
using Unity.Collections;
using Unity.Entities;
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
                Resources.Load("Plate") as GameObject, World.Active);

            networkServerSystem = World.GetOrCreateSystem<NetworkServerSystem>();
        }

        protected override void OnUpdate()
        {
            if (isSpawned)
                return;

            isSpawned = true;

            var query = GetEntityQuery(typeof(TriggerData));

            var entities = query.ToEntityArray(Allocator.TempJob);

            ////TODO 临时在这里生成Table数据
            //var entityList = entities.ToList();
            //entityList.Sort((a, b) => ByteArrayComp.instance.Compare(a, b.netID));
            //networkServerSystem.ReserveSceneEntities(entities.Length);
            //for (var i= 0; i< entities.Length; ++i)
            //{
            //    var entity = entities[i];
            //    var id = networkServerSystem.RegisterEntity(i,(ushort)EntityType.Table, -1, entity);
            //    var replicatedEntityData = EntityManager.GetComponentData<ReplicatedEntityData>(entity);
            //    replicatedEntityData.Id = id;
            //    EntityManager.SetComponentData(entity, replicatedEntityData);
            //}
         
            //生成Plate
            for (var i = 0; i < 3; ++i)
            {
                var entity = entities[i * 2];
                var slot = EntityManager.GetComponentData<SlotPredictedState>(entity);
                //  var pos = EntityManager.GetComponentData<LocalToWorld>(slot.SlotPos);
              

                var e = EntityManager.Instantiate(platePrefab);
                var position = new Translation {Value = slot.SlotPos };
                var rotation = new Rotation {Value = Quaternion.identity};

                EntityManager.SetComponentData(e, position);
                EntityManager.SetComponentData(e, rotation);

                EntityManager.AddComponentData(e, new ReplicatedEntityData()
                {
                    Id = -1,
                    PredictingPlayerId = -1
                });

                EntityManager.AddComponentData(e, new Plate {IsFree = false});

                slot.FilledInEntity = e;
                EntityManager.SetComponentData(entity, slot);

                EntityManager.AddComponentData(e, new ItemInterpolatedState
                {
                    Position = position.Value,
                    Rotation = Quaternion.identity,
                    Owner = Entity.Null
                });


                EntityManager.AddComponentData(e, new ItemPredictedState
                {
                    Position = position.Value,
                    Rotation = Quaternion.identity,
                    Owner = Entity.Null
                });


                var id = networkServerSystem.RegisterEntity(-1,(ushort)EntityType.Plate, -1, e);
                var replicatedEntityData = EntityManager.GetComponentData<ReplicatedEntityData>(e);
                replicatedEntityData.Id = id;
                EntityManager.SetComponentData(e, replicatedEntityData);

            }

            entities.Dispose();
        }
    }
}