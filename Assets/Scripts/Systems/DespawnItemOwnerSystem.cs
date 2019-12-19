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
                ref EntityPredictedState entityPredictedState,
                ref ItemPredictedState predictedState,
                ref ItemInterpolatedState interpolatedState,
                ref LocalToWorld localToWorld) =>
            {
                if (predictedState.Owner == Entity.Null || 
                    replicateEntityServerSystem.HasEntity(predictedState.Owner))
                    return;

                predictedState.Owner = Entity.Null;
                interpolatedState.Owner = Entity.Null;
                entityPredictedState.Transform.pos = localToWorld.Position;
                entityPredictedState.Transform.pos.y = 0.05f;
            });
        }
    }
}