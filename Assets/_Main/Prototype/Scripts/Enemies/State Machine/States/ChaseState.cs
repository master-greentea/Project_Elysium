using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemies
{
    // enemy chases player when it sees them
    public class ChaseState : EnemyState
    {
        public EnemyStateId GetId()
        {
            return EnemyStateId.Chase;
        }
    
        public void Enter(EnemyAgent agent)
        {
            agent.lerpElapsed = 0; // speed lerp
        }
    
        public void Update(EnemyAgent agent)
        {
            agent.SpeedChange(agent.config.chaseSpeed, agent.config.chaseAccelerationDuration);
            if (agent.IsPlayerDetected()) ChasePlayer(agent);
            // if lost player from sight
            else agent.enemyStateMachine.ChangeState(EnemyStateId.Track);
        }
    
        public void Exit(EnemyAgent agent)
        {
            
        }
        
        // methods
        private void ChasePlayer(EnemyAgent agent)
        {
            float distanceFromDestination =
                (agent._playerTransform.position - agent._navMeshAgent.destination).magnitude;
            if (distanceFromDestination > agent.config.maxDistance)
            {
                agent._navMeshAgent.SetDestination(agent._playerTransform.position -
                                                   (agent._playerTransform.position - agent.transform.position)
                                                   .normalized);
            }
            agent.LookAt(agent._playerTransform.position, 1);
        }
    }
}

