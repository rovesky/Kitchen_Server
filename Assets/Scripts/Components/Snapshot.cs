using System;
using System.IO;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.ECS
{
    [InternalBufferCapacity(128)]
    public  unsafe struct SnapshotTick : IBufferElementData
    {
        public uint tick;
        public int length;    
        public uint* data;
    }
    [Serializable]
    public struct Snapshot : IComponentData
    {
        public int i;
    }

}