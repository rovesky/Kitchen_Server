using FootStone.ECS;
using Unity.Entities;

namespace Assets.Scripts.ECS
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
                if (despawn.Frame <= 0)
                {
                    var id = -1;
                    if (EntityManager.HasComponent<Player>(entity))
                        id = EntityManager.GetComponentData<Player>(entity).id;
                    //else if (EntityManager.HasComponent<Enemy>(entity))
                    //    id = EntityManager.GetComponentData<Enemy>(entity).id;
                    if (id != -1)
                        networkServerNewSystem.UnRegisterEntity(id);
                }
            });
        }
    }
}