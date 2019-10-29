using Assets.Scripts.Components;
using FootStone.ECS;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.ECS
{
    [DisableAutoCreation]
    public class SpawnEnemySystem : FSComponentSystem
    {

        private Entity rocket;
        private GameObject enemy1Prefab;
        private GameObject enemy2Prefab;
        private NetworkServerNewSystem networkServerSystem;

        protected override void OnCreate()
        {
            var prefab = Resources.Load("Prefabs/EnemyRocket") as GameObject;       
            rocket = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, World.Active);

            enemy1Prefab = Resources.Load("Prefabs/Enemy1") as GameObject;
            enemy2Prefab = Resources.Load("Prefabs/Enemy3") as GameObject;

            networkServerSystem = World.GetOrCreateSystem<NetworkServerNewSystem>();
        }

        protected override void OnUpdate()
        {
            Entities.ForEach(
               (ref LocalToWorld gunTransform, ref Rotation gunRotation, ref SpawnEnemy spawn) =>
               {
                   var worldTime = GetSingleton<WorldTime>();
                   spawn.spawnTimer -= worldTime.gameTick.TickDuration;
                   if (spawn.spawnTimer > 0)
                       return;

                   spawn.spawnTimer = Random.Range(spawn.spawnIntervalMin, spawn.spawnIntervalMax);

                   var enemyPrefab = spawn.enemyType == EnemyType.Normal ? enemy1Prefab : enemy2Prefab;
                   //    FSLog.Info($"spawn.enemyType:{spawn.enemyType}");


                   var entity = SpawnEntityUtil.SpwanEnemy(EntityManager, enemyPrefab, spawn.enemyType,
                       gunTransform.Position, rocket);

                   EntityManager.AddComponentData(entity, new EntityPredictData()
                   {
                       position = gunTransform.Position,
                       rotation = Quaternion.identity
                   });

                   var typeId = spawn.enemyType == EnemyType.Normal ? 1 : 2;
                   var id = networkServerSystem.RegisterEntity((ushort)typeId, -1, entity);

                   EntityManager.SetComponentData(entity, new Enemy()
                   {
                       id = id,
                       type = spawn.enemyType
                   });
               });
        }
    }
}
