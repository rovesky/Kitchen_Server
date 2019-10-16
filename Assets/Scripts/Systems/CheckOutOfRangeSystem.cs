using FootStone.ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.ECS
{
    [DisableAutoCreation]
    public class CheckOutOfRangeSystem : FSComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAllReadOnly<Enemy>().ForEach((Entity entity,ref Translation translation) =>
            {
                if (translation.Value.z < -7)
                {
                    if (!EntityManager.HasComponent<Despawn>(entity))
                        EntityManager.AddComponentData(entity, new Despawn() { Frame = 0 });

                }
            });

            Entities.WithAllReadOnly<Rocket>().ForEach((Entity entity,ref Rocket rocket, ref Translation translation) =>
            {       
                if (translation.Value.z < -7
                    || translation.Value.z > 5 
                    || translation.Value.x > 7
                    || translation.Value.x < -7)
                {    
                    if (!EntityManager.HasComponent<Despawn>(entity))
                        EntityManager.AddComponentData(entity, new Despawn() { Frame = 0 });
                }                  
            });
        }
    }
}
