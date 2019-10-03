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

            FSLog.Debug("Snapshot create!");
            snapShotQuery = GetEntityQuery(ComponentType.ReadWrite<Snapshot>());

            if (snapShotQuery.CalculateEntityCount() == 0)
            {
                var entity = EntityManager.CreateEntity(typeof(Snapshot));
                snapShotQuery.SetSingleton(new Snapshot());
                EntityManager.AddBuffer<SnapshotTick>(entity);

                FSLog.Debug("Snapshot create1!");
            }
        }

        protected unsafe override void OnUpdate()
        {
            var data = (uint*)UnsafeUtility.Malloc(4 * 1024, UnsafeUtility.AlignOf<UInt32>(), Allocator.Persistent);
            /*
            var writer = new NetworkWriter(data, 4 * 1024, null, false);

            //    FSLog.Debug("Snapshot Update!");      
            tick++;

            writer.WriteUInt32("tick", tick);
            //player
            writer.WriteInt32("playerCount", playerQuery.CalculateEntityCount());
            var playerEntities = playerQuery.ToEntityArray(Allocator.Persistent);
           
            foreach (var entity in playerEntities)
            {
                var player = EntityManager.GetComponentData<Player>(entity);
                writer.WriteInt32("playerId", player.id);

                var pos = EntityManager.GetComponentData<Translation>(entity);
                writer.WriteVector3Q("pos", pos.Value);

                var health = EntityManager.GetComponentData<Health>(entity);
                writer.WriteInt32("health", health.Value);

                var score = EntityManager.GetComponentData<Score>(entity);
                writer.WriteInt32("score", score.ScoreValue);
                writer.WriteInt32("maxScore", score.MaxScoreValue);
            }
            playerEntities.Dispose();

            //ememy
            writer.WriteInt32("enemyCount", enemyQuery.CalculateEntityCount());
            var enemyEntities = enemyQuery.ToEntityArray(Allocator.Persistent);

            foreach (var entity in enemyEntities)
            {
                var enemy = EntityManager.GetComponentData<Enemy>(entity);
                writer.WriteInt32("enemyId", enemy.id);
                writer.WriteByte("enemyType", (byte)enemy.type);

                var pos = EntityManager.GetComponentData<Translation>(entity);
                writer.WriteVector3Q("pos", pos.Value);


                var health = EntityManager.GetComponentData<Health>(entity);
                writer.WriteInt32("health", health.Value);

                var attack = EntityManager.GetComponentData<Attack>(entity);
                writer.WriteInt32("attack", health.Value);

                if (enemy.type == EnemyType.Super)
                {
                    var fireRocket = EntityManager.GetComponentData<FireRocket>(entity);
                    writer.WriteFloatQ("", fireRocket.FireCooldown);
                    writer.WriteFloatQ("", fireRocket.RocketTimer);
                }
            }
            enemyEntities.Dispose();

            //rocket
            writer.WriteInt32("rocketCount", rocketQuery.CalculateEntityCount());
            var rocketEntities = rocketQuery.ToEntityArray(Allocator.Persistent);
            foreach (var entity in rocketEntities)
            {
                var rocekt = EntityManager.GetComponentData<Rocket>(entity);
                writer.WriteInt32("rocketId", rocekt.id);
                writer.WriteByte("", (byte)rocekt.Type);

                var pos = EntityManager.GetComponentData<Translation>(entity);
                writer.WriteVector3Q("pos", pos.Value);

                var health = EntityManager.GetComponentData<Health>(entity);
                writer.WriteInt32("health", health.Value);

                var attack = EntityManager.GetComponentData<Attack>(entity);
                writer.WriteInt32("attack", attack.Power);

                if (rocekt.Type == RocketType.Player)
                {
                    var move = EntityManager.GetComponentData<MoveTranslation>(entity);
                    writer.WriteFloatQ("", move.Speed);
                    writer.WriteByte("", (byte)move.Direction);
                }
                else if (rocekt.Type == RocketType.Enemy)
                {
                    var move = EntityManager.GetComponentData<MoveForward>(entity);
                    writer.WriteFloatQ("", move.Speed);
                }
            }
            rocketEntities.Dispose();
            writer.Flush();*/

            var tempUMS = new UnmanagedMemoryStream((byte*)data, 4 * 1024, 4 * 1024, FileAccess.Write);

            var writer = new BinaryWriter(tempUMS);
            tick++;
            writer.Write(tick);
            //player
            writer.Write(playerQuery.CalculateEntityCount());
            var playerEntities = playerQuery.ToEntityArray(Allocator.Persistent);

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
            var enemyEntities = enemyQuery.ToEntityArray(Allocator.Persistent);

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
            var rocketEntities = rocketQuery.ToEntityArray(Allocator.Persistent);
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
            writer.Flush();

            //snapShot
            var snapshot = snapShotQuery.GetSingletonEntity();
            var buffer = EntityManager.GetBuffer<SnapshotTick>(snapshot);

            if (buffer.Length >= buffer.Capacity)
            {
                UnsafeUtility.Free(buffer[0].data, Allocator.Persistent);
                buffer.RemoveAt(0);
            }
            //     FSLog.Info($"buffer.Length:{buffer.Length},data.length:{snapshotTick.data.ComputeItemCount()}");
            buffer.Add(new SnapshotTick { data = data, tick = tick, length = (int)writer.BaseStream.Position});

        }
    }
}