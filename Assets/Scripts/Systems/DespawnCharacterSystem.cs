using FootStone.ECS;
using Unity.Entities;
using Unity.Mathematics;

namespace FootStone.Kitchen
{
    [DisableAutoCreation]
    public class DespawnCharacterSystem : SystemBase
    {

        protected override void OnUpdate()
        {
            Entities.WithStructuralChanges().
                WithAll<Character,Despawn>().
                ForEach((Entity entity,
                in SlotPredictedState slotState,
                in TransformPredictedState transformState,
                in VelocityPredictedState velocityState) =>
            {
                if(slotState.FilledIn == Entity.Null)
                    return;

                var pickupedEntity = slotState.FilledIn;
                
                var ownerSlot = EntityManager.GetComponentData<SlotSetting>(entity);
                var offset = EntityManager.GetComponentData<OffsetSetting>(pickupedEntity);

                ItemAttachUtilities.ItemDetachFromOwner(EntityManager,
                    pickupedEntity,
                    entity,
                    transformState.Position + math.mul(transformState.Rotation,ownerSlot.Pos + offset.Pos)  ,
                    transformState.Rotation,
                    velocityState.Linear);


              
            }).Run();


        }
    }
}