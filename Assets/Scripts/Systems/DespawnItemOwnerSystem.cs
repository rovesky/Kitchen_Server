﻿using Unity.Entities;
using Unity.Transforms;

namespace FootStone.Kitchen
{
    [DisableAutoCreation]
    public class DespawnItemOwnerSystem : ComponentSystem
    {
     
        private ReplicateEntityServerSystem replicateEntityServerSystem;

        protected override void OnCreate()
        {
            replicateEntityServerSystem = World.GetOrCreateSystem<ReplicateEntityServerSystem>();
        }

        protected override void OnUpdate()
        {

            Entities.ForEach((Entity entity,
                ref ItemPredictedState predictedState,
                ref ItemInterpolatedState interpolatedState,
                ref LocalToWorld localToWorld) =>
            {
                if (predictedState.Owner == Entity.Null || 
                    replicateEntityServerSystem.HasEntity(predictedState.Owner))
                    return;

                predictedState.Owner = Entity.Null;
                interpolatedState.Owner = Entity.Null;
                predictedState.Position = localToWorld.Position;
                predictedState.Position.y = 0.05f;
            });
        }
    }
}