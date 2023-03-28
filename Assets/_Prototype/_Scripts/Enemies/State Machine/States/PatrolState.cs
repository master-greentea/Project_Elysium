using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Enemies
{
    // patrol is entered after track, randomly roam around
    public class PatrolState : EnemyState
    {
        private int roamCount;
        private float checkSeenByPlayerTimer;
        private Transform t;
        private Vector3 forecastDir;

        public EnemyStateId GetId()
        {
            return EnemyStateId.Patrol;
        }
    
        public void Enter(EnemyAgent agent)
        {
            roamCount = 0;
            agent.ChangeSpeed(agent.config.patrolSpeed, agent.config.patrolAcceleration);
            t = agent.transform;
        }
    
        public void Update(EnemyAgent agent)
        {
            // roaming
            Roam(agent);
            // chase player
            if (agent.IsPlayerDetected()) agent.EnemyStateMachine.ChangeState(EnemyStateId.Chase);
            // stare at player (checked based on interval)
            checkSeenByPlayerTimer -= Time.deltaTime;
            if (checkSeenByPlayerTimer < 0)
            {
                checkSeenByPlayerTimer += agent.config.checkSeenByPlayerInterval;
                if (agent.IsSeenByPlayer()) agent.EnemyStateMachine.ChangeState(EnemyStateId.Stare);
            }
            Forecast(agent);
        }

        public void Exit(EnemyAgent agent)
        {
            
        }
        
        // methods
        private void Roam(EnemyAgent agent)
        {
            // if no path, set new destination
            if (!agent.navMeshAgent.hasPath)
            {
                agent.navMeshAgent.SetDestination(agent.RandomWorldPos());
                roamCount++;
            }
            // if have roamed more than x times, enter idle
            if (roamCount > agent.config.maxRoam) agent.EnemyStateMachine.ChangeState(EnemyStateId.Idle);
        }

        private void Forecast(EnemyAgent agent)
        {
            forecastDir = t.forward;
            RaycastHit forcastHit;
            if (Physics.SphereCast(t.position, agent.navMeshAgent.radius, forecastDir, out forcastHit, 3, agent.enemyFov.obstacleLayers))
            {
                agent.forecastDistance = forcastHit.distance;
            }
            else
            {
                agent.forecastDistance = agent.config.patrolSpeed * Services.SkipManager.timeSkipAmount;
            }
            // set forecast transform to sphere position
            agent.forecastTransform.position = t.position + forecastDir * agent.forecastDistance;
            // trigger time skip
            if (agent.IsForecastSeenByPlayer() && SkipManager.canTimeSkip && !SkipManager.isTimeSkipping)
            {
                agent.StartCoroutine(Services.SkipManager.TimeSkip());
            }
        }
    }
}