using UnityEngine;
using Unity.Entities;

namespace Assets.Scripts.ECS
{
    [DisableAutoCreation]
    public class HealthSystem : ComponentSystem
    {

        private void DoDespawan(Entity entity, ref Health health, ref Damage damage, int frame)
        {
            health.Value -= damage.Value;
            damage.Value = 0;
            if (health.Value <= 0)
            {
                if (!EntityManager.HasComponent<Despawn>(entity))
                {
                 //   Debug.Log($"new Despawn,{Time.time}");
                    EntityManager.AddComponentData(entity, new Despawn() {Frame = frame });
                }
            }
        }

        protected override void OnUpdate()
        {
            Entities.WithAllReadOnly<Player>().ForEach((Entity entity, ref Health health, ref Damage damage) =>
            {
                DoDespawan(entity, ref health, ref damage, 1);
            });

            Entities.WithNone<Player>().ForEach((Entity entity, ref Health health, ref Damage damage) =>
            {
                DoDespawan(entity, ref health, ref damage, 0);
            });
        }
    }
}
