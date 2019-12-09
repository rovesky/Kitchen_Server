using FootStone.ECS;
using Unity.Entities;

namespace FootStone.Kitchen
{
    [DisableAutoCreation]
    public class DespawnServerSystem : ComponentSystem
    {
        private NetworkServerSystem networkServerNewSystem;
    
        protected override void OnCreate()
        {
            networkServerNewSystem = World.GetOrCreateSystem<NetworkServerSystem>();
        }

        protected override void OnUpdate()
        {
            Entities.ForEach((Entity entity, ref Despawn despawn) =>
            {
                if (despawn.Frame > 0)
                    return;
                
                if (!EntityManager.HasComponent<ReplicatedEntityData>(entity))
                    return;

                var id = EntityManager.GetComponentData<ReplicatedEntityData>(entity).Id;
                if (id != -1)
                    networkServerNewSystem.UnRegisterEntity(id);
            });
         
        }
    }
}