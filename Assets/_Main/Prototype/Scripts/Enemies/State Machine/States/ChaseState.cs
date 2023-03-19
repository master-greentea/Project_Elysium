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
            agent.ChangeSpeed(agent.config.chaseSpeed, agent.config.chaseAcceleration);
        }
    
        public void Update(EnemyAgent agent)
        {
            if (agent.IsPlayerDetected()) ChasePlayer(agent);
            // if lost player from sight
            else agent.EnemyStateMachine.ChangeState(EnemyStateId.Track);
        }
    
        public void Exit(EnemyAgent agent)
        {
            
        }
        
        // methods
        private void ChasePlayer(EnemyAgent agent)
        {
            float distanceFromDestination =
                (agent.playerTransform.position - agent.navMeshAgent.destination).magnitude;
            if (distanceFromDestination > agent.config.maxDistance)
            {
                agent.navMeshAgent.SetDestination(agent.playerTransform.position -
                                                  (agent.playerTransform.position - agent.transform.position)
                                                  .normalized);
            }
            agent.LookAt(agent.playerTransform.position, 1);
        }
    }
}

