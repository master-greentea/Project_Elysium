using UnityEngine;

namespace Enemies
{
    public class InterceptState : EnemyState
    {
        private float interceptRefreshTimer;
        public EnemyStateId GetId()
        {
            return EnemyStateId.Intercept;
        }

        public void Enter(EnemyAgent agent)
        {
            agent.ChangeSpeed(agent.config.interceptSpeed, agent.config.interceptAccelerationDuration);
        }
        public void Update(EnemyAgent agent)
        {
            if (interceptRefreshTimer < 0)
            {
                interceptRefreshTimer += 1.5f;
                InterceptPlayer(agent);
            }
            interceptRefreshTimer -= Time.deltaTime;
            if (agent.IsPlayerDetected()) agent.EnemyStateMachine.ChangeState(EnemyStateId.Chase);
        }

        public void Exit(EnemyAgent agent)
        {

        }
        
        // methods
        private void InterceptPlayer(EnemyAgent agent)
        {
            Vector3 destination = agent.playerTransform.position + agent.playerTransform.forward * 6f;
            agent.navMeshAgent.SetDestination(destination);
            agent.LookAt(agent.playerTransform.position, 1);
        }
    }
}