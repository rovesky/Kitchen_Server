using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.ECS
{
    [DisableAutoCreation]
    public class GenSnapshotSystem : ComponentSystem
    {
        private EntityQuery playerQuery;
        private EntityQuery enemyQuery;
        private EntityQuery rocketQuery;
        private EntityQuery snapShotQuery;

        private uint tick = 0;

        protected override void OnCreate()
        {
            base.OnCreate();

            playerQuery = GetEntityQuery(ComponentType.ReadOnly<Player>());
            enemyQuery = GetEntityQuery(ComponentType.ReadOnly<Enemy>());
            rocketQuery = GetEntityQuery(ComponentType.ReadOnly<Rocket>());
            snapShotQuery = GetEntityQuery(ComponentType.ReadWrite<Snapshot>());

            if (snapShotQuery.CalculateEntityCount() == 0)
            {
                var entity = EntityManager.CreateEntity(typeof(Snapshot));
                snapShotQuery.SetSingleton(new Snapshot());
                EntityManager.AddBuffer<SnapshotTick>(entity);
            }
        }

        protected unsafe override void OnUpdate()
        {
           
            //var data = (uint*)UnsafeUtility.Malloc(1024,UnsafeUtility.AlignOf<UInt32>(),Allocator.Persistent);

            NativeStream data = new NativeStream(1,Allocator.Persistent);
            var writer = data.AsWriter();
            //nStream.

            //   MemoryStream memStream = new MemoryStream(1024);
            //   BinaryWriter writer = new BinaryWriter(memStream);

            tick++;
            writer.Write(tick);
            //player
            writer.Write(playerQuery.CalculateEntityCount());
            var playerEntities = playerQuery.ToEntityArray(Allocator.Temp);

            foreach (var entity in playerEntities)
            {
                var player = EntityManager.GetComponentData<Player>(entity);
                writer.Write(player.id);

                var pos = EntityManager.GetComponentData<Translation>(entity);
                writer.Write(pos.Value.x);
                writer.Write(pos.Value.y);
                writer.Write(pos.Value.z);

                var health = EntityManager.GetComponentData<Health>(entity);
                writer.Write(health.Value);

                var score = EntityManager.GetComponentData<Score>(entity);
                writer.Write(score.ScoreValue);
                writer.Write(score.MaxScoreValue);
            }
            playerEntities.Dispose();

            //ememy
            writer.Write(enemyQuery.CalculateEntityCount());
            var enemyEntities = enemyQuery.ToEntityArray(Allocator.Temp);

            foreach (var entity in enemyEntities)
            {
                var enemy = EntityManager.GetComponentData<Enemy>(entity);
                writer.Write(enemy.id);
                writer.Write((byte)enemy.type);

                var pos = EntityManager.GetComponentData<Translation>(entity);
                writer.Write(pos.Value.x);
                writer.Write(pos.Value.y);
                writer.Write(pos.Value.z);

                var health = EntityManager.GetComponentData<Health>(entity);
                writer.Write(health.Value);

                var attack = EntityManager.GetComponentData<Attack>(entity);
                writer.Write(attack.Power);

                if (enemy.type == EnemyType.Super)
                {
                    var fireRocket = EntityManager.GetComponentData<FireRocket>(entity);
                    writer.Write(fireRocket.FireCooldown);
                    writer.Write(fireRocket.RocketTimer);
                }
            }
            enemyEntities.Dispose();

            //rocket
            writer.Write(rocketQuery.CalculateEntityCount());
            var rocketEntities = rocketQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in rocketEntities)
            {
                var rocekt = EntityManager.GetComponentData<Rocket>(entity);
                writer.Write(rocekt.id);
                writer.Write((byte)rocekt.Type);

                var pos = EntityManager.GetComponentData<Translation>(entity);
                writer.Write(pos.Value.x);
                writer.Write(pos.Value.y);
                writer.Write(pos.Value.z);

                var health = EntityManager.GetComponentData<Health>(entity);
                writer.Write(health.Value);

                var attack = EntityManager.GetComponentData<Attack>(entity);

                if (rocekt.Type == RocketType.Player)
                {
                    var move = EntityManager.GetComponentData<MoveTranslation>(entity);
                    writer.Write(move.Speed);
                    writer.Write((byte)move.Direction);
                }
                else if (rocekt.Type == RocketType.Enemy)
                {
                    var move = EntityManager.GetComponentData<MoveForward>(entity);
                    writer.Write(move.Speed);
                }
            }
            rocketEntities.Dispose();

            //snapShot
            var snapshot = snapShotQuery.GetSingletonEntity();
            var buffer = EntityManager.GetBuffer<SnapshotTick>(snapshot);

            if (buffer.Length >= buffer.Capacity)
            {
                buffer.RemoveAt(0);
            }

           
        //    buffer.Add(new SnapshotTick() { data = data });

            
        }
    }
}