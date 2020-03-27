using FootStone.ECS;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace FootStone.Kitchen
{
    [DisableAutoCreation]
    public class SpawnRobotsSystem : ComponentSystem
    {
        private bool isSpawned;

        protected override void OnUpdate()
        {
            if (isSpawned)
                return;

            isSpawned = true;

            var entity = GetSingletonEntity<SpawnPlayerServer>();
            var buffer = EntityManager.GetBuffer<SpawnPlayerBuffer>(entity);
            buffer.Add(new SpawnPlayerBuffer
            {
                PlayerId = -2,
                IsRobot = true,
                Position = new float3(2, 1,  -5)
            });
           
        }
    }
}