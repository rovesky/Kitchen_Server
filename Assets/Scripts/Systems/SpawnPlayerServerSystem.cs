﻿using Unity.Entities;
using Unity.Physics.Authoring;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.ECS
{  
    [DisableAutoCreation]
    public class SpawnPlayerServerSystem : ComponentSystem
    {
        private Entity rocket;
        private Entity player;
        private EntityQuery spawnPlayerQuery;

        protected override void OnCreate()
        {
            spawnPlayerQuery = GetEntityQuery(ComponentType.ReadOnly<SpawnPlayerServer>());
            var entity = EntityManager.CreateEntity(typeof(SpawnPlayerServer));
            spawnPlayerQuery.SetSingleton(new SpawnPlayerServer());
            EntityManager.AddBuffer<SpawnPlayerBuffer>(entity);

            rocket = GameObjectConversionUtility.ConvertGameObjectHierarchy(
                Resources.Load("Prefabs/Rocket") as GameObject, World.Active);

            player = GameObjectConversionUtility.ConvertGameObjectHierarchy(
                Resources.Load("Prefabs/Player") as GameObject, World.Active);
        }

        protected override void OnUpdate()
        {
            var entity = spawnPlayerQuery.GetSingletonEntity();

            var buffer = EntityManager.GetBuffer<SpawnPlayerBuffer>(entity);
            if (buffer.Length == 0)
                return;

            var array = buffer.ToNativeArray(Unity.Collections.Allocator.Temp);
            buffer.Clear();

            for (int i = 0; i < array.Length; ++i)
            {
                var playerBuffer = array[i];

                //创建Player
                var e = EntityManager.Instantiate(player);
                Translation position = new Translation() { Value = Vector3.zero };

                Quaternion r = Quaternion.identity;
                r.eulerAngles = new Vector3(0, -180, 0);
                Rotation rotation = new Rotation() { Value = r};

                //   rotation.Value.value.y = -180;

                EntityManager.SetComponentData(e, position);
                EntityManager.SetComponentData(e, rotation);
                EntityManager.AddComponentData(e, new Player() { playerId = playerBuffer.playerId, id = e.Index });
                EntityManager.AddComponentData(e, new Attack() { Power = 10000 });
                EntityManager.AddComponentData(e, new Damage());
                EntityManager.AddComponentData(e, new Health() { Value = 30 });
                EntityManager.AddComponentData(e, new Score() { ScoreValue = 0, MaxScoreValue = 0 });
                EntityManager.AddComponentData(e, new UpdateUI());

                EntityManager.AddComponentData(e, new FireRocket()
                {
                    Rocket = rocket,
                    FireCooldown = 0.1f,
                    RocketTimer = 0,
                });
                EntityManager.AddComponentData(e, new MovePosition()
                {
                    Speed = 5,
                });

                EntityManager.AddComponentData(e, new PlayerCommand()
                {
                    renderTick = 0,
                    targetPos = Vector3.zero

                });

                EntityManager.AddComponentData(e, new Connection()
                {
                    id = playerBuffer.playerId,              

                });

            }             
        }
    }
}
