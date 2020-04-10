using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace FootStone.Kitchen
{
    [InternalBufferCapacity(16)]
    public struct SpawnPlayerBuffer : IBufferElementData
    {
        public int PlayerId;
        public float3 Position;
        public bool IsRobot;
    }

    public struct SpawnPlayerServer : IComponentData
    {
    }
}