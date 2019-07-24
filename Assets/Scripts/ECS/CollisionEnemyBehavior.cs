﻿using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

namespace Assets.Scripts.ECS
{
    [Serializable]
    public struct CollisionEnemy : IComponentData
    {   

    }

    [RequiresEntityConversion]
    public class CollisionEnemyBehavior : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if (enabled)
            {
                dstManager.AddComponentData(entity, new CollisionEnemy());
            }
        }
    }



    // This system applies an impulse to any dynamic that collides with a Repulsor.
    // A Repulsor is defined by a PhysicsShape with the `Raise Collision Events` flag ticked and a
    // CollisionEventImpulse behaviour added.
    [UpdateAfter(typeof(EndFramePhysicsSystem))]
    public class CollisionEnemySystem : JobComponentSystem
    {
        BuildPhysicsWorld m_BuildPhysicsWorldSystem;
        StepPhysicsWorld m_StepPhysicsWorldSystem;
     //   EndSimulationEntityCommandBufferSystem m_EntityCommandBufferSystem;

        //EntityQuery ImpulseGroup;

        protected override void OnCreate()
        {
            m_BuildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
            m_StepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();
        //    m_EntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            //ImpulseGroup = GetEntityQuery(new EntityQueryDesc
            //{
            //    All = new ComponentType[] { typeof(CollisionEnemy), }
            //});
        }

       // [BurstCompile]
        struct CollisionEventImpulseJob : ITriggerEventsJob
        {
            [ReadOnly] public ComponentDataFromEntity<CollisionEnemy> ColliderEventImpulseGroup;
            public ComponentDataFromEntity<PhysicsVelocity> PhysicsVelocityGroup;
            public ComponentDataFromEntity<EntityKiller> LifeTimeGroup;
        //    public EntityCommandBuffer CommandBuffer;

            public void Execute(TriggerEvent triggerEvent)
            {
             
                Entity entityA = triggerEvent.Entities.EntityA;
                Entity entityB = triggerEvent.Entities.EntityB;

                bool isBodyATrigger = ColliderEventImpulseGroup.Exists(entityA);
                bool isBodyBTrigger = ColliderEventImpulseGroup.Exists(entityB);

                // Ignoring Triggers overlapping other Triggers
                if (isBodyATrigger && isBodyBTrigger)
                    return;

                bool isBodyADynamic = PhysicsVelocityGroup.Exists(entityA);
                bool isBodyBDynamic = PhysicsVelocityGroup.Exists(entityB);

                // Ignoring overlapping static bodies
                if ((isBodyATrigger && !isBodyBDynamic) ||
                    (isBodyBTrigger && !isBodyADynamic))
                    return;

                var triggerEntity = isBodyATrigger ? entityA : entityB;
                var dynamicEntity = isBodyATrigger ? entityB : entityA;

                //碰撞直接死亡
                //   CommandBuffer.DestroyEntity(triggerEntity);

                var component = LifeTimeGroup[triggerEntity];
              //  Debug.Log($"TriggerEvent dead{component.TimeToDie}");
                component.TimeToDie = 0;
                LifeTimeGroup[triggerEntity] = component;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            // Debug.Log("CollisionEnemySystem OnUpdate!");
            JobHandle jobHandle = new CollisionEventImpulseJob
            {
                ColliderEventImpulseGroup = GetComponentDataFromEntity<CollisionEnemy>(true),
                PhysicsVelocityGroup = GetComponentDataFromEntity<PhysicsVelocity>(),
                LifeTimeGroup = GetComponentDataFromEntity<EntityKiller>(),
           //     CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer(),
            }.Schedule(m_StepPhysicsWorldSystem.Simulation,
                        ref m_BuildPhysicsWorldSystem.PhysicsWorld, inputDeps);

            return jobHandle;
        }
    }

}