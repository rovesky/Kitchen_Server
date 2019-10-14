using Assets.Scripts.Components;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.ECS
{
    [DisableAutoCreation]
    public class SpawnEnemySystem : ComponentSystem
    {

        private Entity rocket;
        private GameObject enemy1Prefab;
        private GameObject enemy2Prefab;

        protected override void OnCreate()
        {
            var prefab = Resources.Load("Prefabs/EnemyRocket") as GameObject;       
            rocket = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, World.Active);

            enemy1Prefab = Resources.Load("Prefabs/Enemy1") as GameObject;
            enemy2Prefab = Resources.Load("Prefabs/Enemy3") as GameObject;
        }

        protected override void OnUpdate()
        {
            Entities.ForEach(
               (ref LocalToWorld gunTransform, ref Rotation gunRotation, ref SpawnEnemy spawn) =>
               {      

                   spawn.spawnTimer -= Time.deltaTime;
                   if (spawn.spawnTimer > 0)
                       return;

                   spawn.spawnTimer = Random.Range(spawn.spawnIntervalMin, spawn.spawnIntervalMax);
                
                   var enemyPrefab = spawn.enemyType == EnemyType.Normal? enemy1Prefab:enemy2Prefab;

                   var entity = SpawnEntityUtil.SpwanEnemy(EntityManager, enemyPrefab, spawn.enemyType, 
                       gunTransform.Position, rocket);
                  
               });
        }
    }
}
