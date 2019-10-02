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
        public int tick;
        public int length;    
       // public NativeStream data;
    }
    [Serializable]
    public struct Snapshot : IComponentData
    {
        public int i;
    }

}