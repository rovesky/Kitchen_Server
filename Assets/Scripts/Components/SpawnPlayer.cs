using Unity.Entities;

namespace Assets.Scripts.ECS
{
    [InternalBufferCapacity(16)]
    public struct SpawnPlayerBuffer : IBufferElementData
    {
        public int PlayerId;
        public int ConnectionId;
    }

    public struct SpawnPlayerServer : IComponentData
    {
    }
}