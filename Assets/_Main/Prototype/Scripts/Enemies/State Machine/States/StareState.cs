using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemies
{
    // idle state, randomly look around
    public class StareState : EnemyState
    {
        public EnemyStateId GetId()
        {
            return EnemyStateId.Stare;
        }
    
        public void Enter(EnemyAgent agent)
        {
            agent.StartCoroutine(TurnToPlayer(agent));
            agent.navMeshAgent.destination = agent.transform.position; // stop self movement
        }
    
        public void Update(EnemyAgent agent)
        {
            // chase player
            if (agent.IsPlayerDetected()) agent.EnemyStateMachine.ChangeState(EnemyStateId.Chase);
        }

        public void Exit(EnemyAgent agent)
        {
            
        }

        IEnumerator TurnToPlayer(EnemyAgent agent)
        {
            yield return new WaitForSeconds(agent.config.delayBeforeTurnToPlayer);
            agent.LookAt(agent.playerTransform.position, 10); // immediately looks at player
            // do stuff
            
            yield return new WaitForSeconds(agent.config.delayBeforeTrackPlayer); // short delay then track
            agent.EnemyStateMachine.ChangeState(EnemyStateId.Track);
        }
    }
}