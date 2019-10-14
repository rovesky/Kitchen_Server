using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.ECS
{
    // [UpdateInGroup(typeof(InitializationSystemGroup))]
    [DisableAutoCreation]
    public class ResetWorldSystem : ComponentSystem
    {
        public bool IsReset = false;
        protected override void OnUpdate()
        {
            if(!IsReset)
                return;
            IsReset = false;

            foreach (var entity in EntityManager.GetAllEntities())
            {
                EntityManager.DestroyEntity(entity);
            }
        }
    }
}