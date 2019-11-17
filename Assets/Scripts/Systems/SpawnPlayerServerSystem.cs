﻿using FootStone.ECS;
using Unity.Entities;
using Unity.Physics.Authoring;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.ECS
{  
    [DisableAutoCreation]
    public class SpawnPlayerServerSystem : ComponentSystem
    {
     //   private Entity rocket;
        private Entity playerPrefab;
        private NetworkServerSystem networkServerSystem;
        protected override void OnCreate()
        {  
             var entity = EntityManager.CreateEntity(typeof(SpawnPlayerServer));
             SetSingleton(new SpawnPlayerServer());
             EntityManager.AddBuffer<SpawnPlayerBuffer>(entity);

            //rocket = GameObjectConversionUtility.ConvertGameObjectHierarchy(
            //    Resources.Load("Prefabs/Rocket") as GameObject, World.Active);

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

            var array = buffer.ToNativeArray(Unity.Collections.Allocator.Temp);
            buffer.Clear();

            for (int i = 0; i < array.Length; ++i)
            {
                var playerBuffer = array[i];
               
                //创建Player
                var e = EntityManager.Instantiate(playerPrefab);

                var id = networkServerSystem.RegisterEntity(0, playerBuffer.playerId, e);

                Translation position = new Translation() { Value = { x = 0, y = 1, z = -5 } };
                Rotation rotation = new Rotation() { Value = Quaternion.identity };
                
                EntityManager.SetComponentData(e, position);
                EntityManager.SetComponentData(e, rotation);
                EntityManager.AddComponentData(e, new Player() { playerId = playerBuffer.playerId, id = id });
             //   EntityManager.AddComponentData(e, new Attack() { Power = 10000 });
              //  EntityManager.AddComponentData(e, new Damage());
              //  EntityManager.AddComponentData(e, new Health() { Value = 30 });
             //   EntityManager.AddComponentData(e, new Score() { ScoreValue = 0, MaxScoreValue = 10 });
				EntityManager.AddComponentData(e, new CharacterDataComponent()
				{
					SkinWidth = 0.02f,
					Entity = e,
				});

			
				EntityManager.AddComponentData(e, new MoveInput()
                {
                    Speed = 6,
                });

                EntityManager.AddComponentData(e, new UserCommand()
                {
                    renderTick = 0,
                    targetPos = Vector3.zero

                });
                EntityManager.AddBuffer<UserCommandBuffer>(e);

                EntityManager.AddComponentData(e, new Connection()
                {
                    id = playerBuffer.playerId,
                    sessionId  = playerBuffer.playerId

                });

                EntityManager.AddComponentData(e, new CharacterInterpolateState()
                {
                    position = position.Value,
                    rotation = rotation.Value

                });

                EntityManager.AddComponentData(e, new CharacterPredictState()
                {
                    position = position.Value,
                    rotation = rotation.Value,
                    pickupEntity = Entity.Null             

                });

                EntityManager.AddComponentData(e, new PickupItem());

                EntityManager.AddComponentData(e, new ThrowItem()
                {
                    speed = 14
                });

            }

            array.Dispose();
        }
    }
}
