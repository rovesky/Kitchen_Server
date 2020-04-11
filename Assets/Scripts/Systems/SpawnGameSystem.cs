using System;
using FootStone.ECS;
using Unity.Entities;

namespace FootStone.Kitchen
{
    [DisableAutoCreation]
    public class SpawnGameSystem : SystemBase
    {
        private bool isSpawned;
        private NetworkServerSystem networkServerSystem;
        public const ushort TotalTime = 300;
        protected override void OnCreate()
        {
         
            networkServerSystem = World.GetOrCreateSystem<NetworkServerSystem>();
        }

        protected override void OnUpdate()
        {
            if (isSpawned)
                return;

            isSpawned = true;

            var e = EntityManager.CreateEntity(typeof(ReplicatedEntityData),typeof(Countdown),typeof(Score));
            var id = networkServerSystem.RegisterEntity(-1, (ushort)EntityType.Game, -1, e);
            EntityManager.SetComponentData(e,new ReplicatedEntityData()
            {
                Id = id,
                PredictingPlayerId = -1
            });

            EntityManager.SetComponentData(e,new Countdown()
            {
                Value = TotalTime,
                EndTime = DateTime.Now.AddSeconds(TotalTime).Ticks
            });

            EntityManager.SetComponentData(e,new Score()
            {
                Value = 0
            });
        }
    }
}