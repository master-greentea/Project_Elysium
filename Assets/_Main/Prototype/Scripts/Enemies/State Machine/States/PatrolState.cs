using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemies
{
    // patrol is entered after track, randomly roam around
    public class PatrolState : EnemyState
    {
        private int roamCount;

        public EnemyStateId GetId()
        {
            return EnemyStateId.Patrol;
        }
    
        public void Enter(EnemyAgent agent)
        {
            agent.lerpElapsed = 0; // speed lerp
            roamCount = 0;
        }
    
        public void Update(EnemyAgent agent)
        {
            agent.SpeedChange(agent.config.patrolSpeed, agent.config.patrolAccelerationDuration);
            
            // roaming
            if (!agent._navMeshAgent.hasPath)
            {
                agent._navMeshAgent.SetDestination(agent.RandomWorldPos());
                roamCount++;
            }
            if (roamCount > agent.config.maxRoam) agent.enemyStateMachine.ChangeState(EnemyStateId.Idle);

            // chase player
            if (agent.IsPlayerDetected()) agent.enemyStateMachine.ChangeState(EnemyStateId.Chase);
        }

        public void Exit(EnemyAgent agent)
        {
            
        }
        
        // methods
    }
}