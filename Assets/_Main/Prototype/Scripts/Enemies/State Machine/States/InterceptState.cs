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
            agent.lerpElapsed = 0; // speed lerp
        }
        public void Update(EnemyAgent agent)
        {
            agent.SpeedChange(agent.config.interceptSpeed, agent.config.interceptAccelerationDuration);
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