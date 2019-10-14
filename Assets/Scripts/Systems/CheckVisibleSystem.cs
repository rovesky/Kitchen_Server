//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Unity.Entities;
//using UnityEngine;

//namespace Assets.Scripts.ECS
//{
//    [DisableAutoCreation]
//    public class CheckVisibleSystem : ComponentSystem
//    {
//        protected override void OnUpdate()
//        {
//            Entities.ForEach((CheckVisibleBehaviour behaviour) =>
//            {
//                if (behaviour.InVisible())
//                {
//                    if (!EntityManager.HasComponent<Despawn>(behaviour.entity))
//                        EntityManager.AddComponentData(behaviour.entity, new Despawn() { Frame = 0 });
//                }
//            });
//        }
//    }
//}
