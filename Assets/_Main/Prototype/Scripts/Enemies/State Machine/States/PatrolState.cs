using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemies
{
    // patrol is entered after track, randomly roam around
    public class PatrolState : EnemyState
    {
        private int roamCount;
        private float checkSeenByPlayerTimer;

        public EnemyStateId GetId()
        {
            return EnemyStateId.Patrol;
        }
    
        public void Enter(EnemyAgent agent)
        {
            roamCount = 0;
            agent.ChangeSpeed(agent.config.patrolSpeed, agent.config.patrolAccelerationDuration);
        }
    
        public void Update(EnemyAgent agent)
        {
            // roaming
            if (!agent.navMeshAgent.hasPath)
            {
                agent.navMeshAgent.SetDestination(agent.RandomWorldPos());
                roamCount++;
            }
            if (roamCount > agent.config.maxRoam) agent.EnemyStateMachine.ChangeState(EnemyStateId.Idle);
            // chase player
            if (agent.IsPlayerDetected()) agent.EnemyStateMachine.ChangeState(EnemyStateId.Chase);
            // stare at player (checked based on interval)
            checkSeenByPlayerTimer -= Time.deltaTime;
            if (checkSeenByPlayerTimer < 0)
            {
                checkSeenByPlayerTimer += agent.config.checkSeenByPlayerInterval;
                if (agent.IsSeenByPlayer()) agent.EnemyStateMachine.ChangeState(EnemyStateId.Stare);
            }
        }

        public void Exit(EnemyAgent agent)
        {
            
        }
        
        // methods
    }
}