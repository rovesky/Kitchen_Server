using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace Assets.Scripts.ECS
{
    [UnityEngine.ExecuteAlways]
    public class ServerSimulationSystemGroup : ComponentSystemGroup
    {
        protected override void OnCreate()
        {

            m_systemsToUpdate.Add(World.GetOrCreateSystem<NetworkServerSystem>());
            //  m_systemsToUpdate.Add(World.GetOrCreateSystem<InputSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystem<SpawnPlayerSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystem<PlayerFireSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystem<EnemyFireSystem>());

            m_systemsToUpdate.Add(World.GetOrCreateSystem<RayCastSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystem<HealthSystem>()); 

        }

        public override void SortSystemUpdateList()
        {
           
        }
    }
}
