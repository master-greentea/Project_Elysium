using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemies
{
    // idle state, randomly look around
    public class IdleState : EnemyState
    {
        private bool resting;
        private Vector3 randLookAt;
        private int restCount;
        
        public EnemyStateId GetId()
        {
            return EnemyStateId.Idle;
        }
    
        public void Enter(EnemyAgent agent)
        {
            restCount = 0;
        }
    
        public void Update(EnemyAgent agent)
        {
            Rest(agent);
            // chase player
            if (agent.IsPlayerDetected()) agent.enemyStateMachine.ChangeState(EnemyStateId.Chase);
        }

        public void Exit(EnemyAgent agent)
        {
            
        }
        
        // methods
        private void Rest(EnemyAgent agent)
        {
            agent.LookAt(randLookAt, agent.config.restLookSpeed);
            // look at random direction after interval
            agent.config.restTimer -= Time.deltaTime;
            if (agent.config.restTimer < 0)
            {
                agent.config.restTimer += agent.config.restLookInterval;
                randLookAt = agent.RandomWorldPos();
                restCount++;
            }
            // if have looked more than x times, enter patrol
            if (restCount > agent.config.maxRest)
            {
                agent.enemyStateMachine.ChangeState(EnemyStateId.Patrol);
            }
        }
    }
}