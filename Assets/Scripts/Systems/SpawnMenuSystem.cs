using System.Collections.Generic;
using FootStone.ECS;
using Unity.Collections;
using Unity.Entities;

namespace FootStone.Kitchen
{
    [DisableAutoCreation]
    public class SpawnMenuSystem : SystemBase
    {
        private NetworkServerSystem networkServerSystem;

        private Dictionary<MenuType,Menu> menuTemplate = new Dictionary<MenuType, Menu>();
   
        protected override void OnCreate()
        {
            var entity = EntityManager.CreateEntity(typeof(SpawnMenuArray));
            SetSingleton(new SpawnMenuArray());
            EntityManager.AddBuffer<SpawnMenuRequest>(entity);

            RegisterMenu(MenuType.Shrimp, 1,5);
            RegisterMenu(MenuType.Sushi, 2,6,1,7);

            networkServerSystem = World.GetOrCreateSystem<NetworkServerSystem>();
        }

        private void RegisterMenu(MenuType type, int productId, 
            int material1, int material2 = 0, int material3 = 0, int material4 = 0)
        {
            menuTemplate.Add(type,new Menu()
            {
                Index = 0,
                ProductId = (ushort)productId,
                MaterialId1 = (ushort)material1,
                MaterialId2 = (ushort)material2,
                MaterialId3 = (ushort)material3,
                MaterialId4 = (ushort)material4,
            });
        }

        protected override void OnUpdate()
        {
           
            var entity = GetSingletonEntity<SpawnMenuArray>();
            var buffer = EntityManager.GetBuffer<SpawnMenuRequest>(entity);
            if (buffer.Length == 0)
                return;

            var array = buffer.ToNativeArray(Allocator.Temp);
            buffer.Clear();

            foreach (var spawnMenu in array)
            {
                if(!menuTemplate.ContainsKey(spawnMenu.Type))
                    continue;

                var e = EntityManager.CreateEntity(typeof(ReplicatedEntityData), typeof(Menu));
                var id = networkServerSystem.RegisterEntity(-1, (ushort) EntityType.Menu, -1, e);
                EntityManager.SetComponentData(e, new ReplicatedEntityData()
                {
                    Id = id,
                    PredictingPlayerId = -1
                });

                var menu = menuTemplate[spawnMenu.Type];
                menu.Index = spawnMenu.index;
                EntityManager.SetComponentData(e, menu);
            }
            array.Dispose();
        }
    }
}