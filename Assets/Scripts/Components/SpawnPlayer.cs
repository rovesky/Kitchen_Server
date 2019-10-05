using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.ECS
{
    [InternalBufferCapacity(10)]
    public struct PlayerBuffer : IBufferElementData
    {
        public int playerId;
    }

    [Serializable]
    public struct SpawnPlayerServer : IComponentData
    {
       // public Entity entity;
       // public bool spawn;
    }

}