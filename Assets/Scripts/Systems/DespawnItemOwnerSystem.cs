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

            //Entities.ForEach((Entity entity,
            //    ref TransformPredictedState transformPredictedState,
            //    ref OwnerPredictedState ownerState,
            //    // ref ItemInterpolatedState interpolatedState,
            //    ref LocalToWorld localToWorld) =>
            //{
            //    if (ownerState.Owner == Entity.Null ||
            //         replicateEntityServerSystem.HasEntity(ownerState.Owner))
            //        return;

            //    if (!EntityManager.HasComponent<Character>(ownerState.Owner))
            //        return;

            //    ownerState.Owner = Entity.Null;
            //    ownerState.PreOwner = Entity.Null;
            //    //  interpolatedState.Owner = Entity.Null;
            //    transformPredictedState.Position = localToWorld.Position;
            //    transformPredictedState.Position.y = 0.05f;
            //});


            Entities.ForEach((Entity entity,
                ref OwnerPredictedState ownerState
              ) =>
            {
                if (ownerState.PreOwner == Entity.Null ||
                    replicateEntityServerSystem.HasEntity(ownerState.PreOwner))
                    return;

                ownerState.PreOwner = Entity.Null;

            });
        }
    }
}