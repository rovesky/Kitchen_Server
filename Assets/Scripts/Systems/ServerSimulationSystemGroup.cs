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

            m_systemsToUpdate.Add(World.GetOrCreateSystem<SpawnEnemySystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystem<SpawnPlayerServerSystem>());

            m_systemsToUpdate.Add(World.GetOrCreateSystem<PlayerFireSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystem<EnemyFireSystem>());

            m_systemsToUpdate.Add(World.GetOrCreateSystem<MoveForwardSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystem<MoveSinSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystem<MovePositionSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystem<MoveTargetSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystem<MoveTranslationSystem>());

            m_systemsToUpdate.Add(World.GetOrCreateSystem<CheckVisibleSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystem<RayCastSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystem<HealthSystem>());

            m_systemsToUpdate.Add(World.GetOrCreateSystem<GenSnapshotSystem>());
            m_systemsToUpdate.Add(World.GetOrCreateSystem<SendDataSystem>());
        }

        public override void SortSystemUpdateList()
        {
           
        }
    }
}
