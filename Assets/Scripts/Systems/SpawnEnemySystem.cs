﻿using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.ECS
{

    public class SpawnEnemySystem : ComponentSystem
    {

        protected override void OnUpdate()
        {
            Entities.ForEach(
               (ref LocalToWorld gunTransform, ref Rotation gunRotation, ref SpawnEnemy spawn) =>
               {
                   if (spawn.entity == null)
                       return;

                   spawn.spawnTimer -= Time.deltaTime;
                   if (spawn.spawnTimer > 0)
                       return;

                   spawn.spawnTimer = Random.Range(spawn.spawnIntervalMin, spawn.spawnIntervalMax);

                   var e = PostUpdateCommands.Instantiate(spawn.entity);

                   Translation position = new Translation() { Value = gunTransform.Position };
                   Rotation rotation = new Rotation() { Value = Quaternion.identity };

                   PostUpdateCommands.SetComponent(e, position);
                   PostUpdateCommands.SetComponent(e, rotation);

                   PostUpdateCommands.AddComponent(e, new Enemy());
                   PostUpdateCommands.AddComponent(e, new Damage());
                   PostUpdateCommands.AddComponent(e, new Attack() { Power = 1 });
                   PostUpdateCommands.AddComponent(e, new MoveTranslation() { Speed = 1, Direction = Direction.Down });


                   if (spawn.enemyType == EnemyType.Normal)
                   {
                       PostUpdateCommands.AddComponent(e, new Health() { Value = 100 });
                       PostUpdateCommands.AddComponent(e, new MoveSin() );
                  
                   }
                   else
                   {
                       PostUpdateCommands.AddComponent(e, new Health() { Value = 500 });                   
                   }

               }
           );
        }
    }
}