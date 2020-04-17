using FootStone.ECS;
using Unity.Entities;


namespace FootStone.Kitchen
{
    [DisableAutoCreation]
    public class RegisterEntityToNetwork : SystemBase
    {
        private NetworkServerSystem networkServerSystem;
        protected override void OnCreate()
        {

            networkServerSystem = World.GetOrCreateSystem<NetworkServerSystem>();
        }

        protected override void OnUpdate()
        {
            Entities
                .WithStructuralChanges()
                .ForEach((Entity entity,
                    ref  ReplicatedEntityData   replicatedData,
                    in GameEntity gameEntity) =>
                {
                    if (replicatedData.Id != -1)
                        return;
                

                    var id = networkServerSystem.RegisterEntity(-1, (ushort)gameEntity.Type,
                        replicatedData.PredictingPlayerId, entity);
                    replicatedData.Id = id;

                }).Run();
        }
    }
}