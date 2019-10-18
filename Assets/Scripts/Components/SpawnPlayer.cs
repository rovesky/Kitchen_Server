using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.ECS
{
    [InternalBufferCapacity(16)]
    public struct SpawnPlayerBuffer : IBufferElementData
    {
        public int playerId;
        public int connectionId;
    }

    [Serializable]
    public struct SpawnPlayerServer : IComponentData
    {    

    }

}