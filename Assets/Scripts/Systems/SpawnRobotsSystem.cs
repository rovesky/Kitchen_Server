using Unity.Entities;
using Unity.Mathematics;

namespace FootStone.Kitchen
{
    [DisableAutoCreation]
    public class SpawnRobotsSystem : ComponentSystem
    {
        private bool isSpawned;

        protected override void OnUpdate()
        {
           return;
            if (isSpawned)
                return;

            isSpawned = true;

            var entity = GetSingletonEntity<SpawnPlayerServer>();
            var buffer = EntityManager.GetBuffer<SpawnPlayerBuffer>(entity);
            buffer.Add(new SpawnPlayerBuffer
            {
                PlayerId = 3,
                IsRobot = true,
                Position = new float3(2, 1,  -5)
            });
           
        }
    }
}