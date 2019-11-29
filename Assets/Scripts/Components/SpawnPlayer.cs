using Unity.Entities;

namespace FootStone.Kitchen
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