using Unity.Entities;
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
                ref TransformPredictedState transformPredictedState,
                ref ItemPredictedState predictedState,
                ref ItemInterpolatedState interpolatedState,
                ref LocalToWorld localToWorld) =>
            {
                if (predictedState.Owner == Entity.Null ||
                     replicateEntityServerSystem.HasEntity(predictedState.Owner))
                return;

                if(!EntityManager.HasComponent<Character>(predictedState.Owner))
                    return;

                predictedState.Owner = Entity.Null;
                predictedState.PreOwner = Entity.Null;
                interpolatedState.Owner = Entity.Null;
                transformPredictedState.Position = localToWorld.Position;
                transformPredictedState.Position.y = 0.05f;
            });


            Entities.ForEach((Entity entity,
                ref ItemPredictedState predictedState
              ) =>
            {
                if (predictedState.PreOwner == Entity.Null ||
                    replicateEntityServerSystem.HasEntity(predictedState.PreOwner))
                    return;

                predictedState.PreOwner = Entity.Null;

            });
        }
    }
}