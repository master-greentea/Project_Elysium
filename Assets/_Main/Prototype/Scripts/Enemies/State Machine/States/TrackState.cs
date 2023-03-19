using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemies
{
    // track is immediately entered when enemy loses sight of player
    public class TrackState : EnemyState
    {
        private int currentTrackIndex;
        
        public EnemyStateId GetId()
        {
            return EnemyStateId.Track;
        }
    
        public void Enter(EnemyAgent agent)
        {
            ResetTrack(agent);
            agent.ChangeSpeed(agent.config.trackSpeed, agent.config.trackAcceleration);
        }
    
        public void Update(EnemyAgent agent)
        {
            // map track overtime
            if (!agent.trackMapped) agent.MapPlayerTrack();
            // follow track
            FollowMappedTrack(agent);
            // chase player
            if (agent.IsPlayerDetected()) agent.EnemyStateMachine.ChangeState(EnemyStateId.Chase);
            // intercept player if player close by (?)
            // if ((agent.transform.position - agent.playerTransform.position).magnitude <= 5f)
            //     agent.EnemyStateMachine.ChangeState(EnemyStateId.Intercept);
        }

        public void Exit(EnemyAgent agent)
        {
            ResetTrack(agent);
        }
        
        // methods
        private void ResetTrack(EnemyAgent agent)
        {
            agent.mappedPlayerPositions.Clear();
            agent.trackMapped = false;
            currentTrackIndex = 0;
        }
        private void FollowMappedTrack(EnemyAgent agent)
        {
            if (agent.mappedPlayerPositions.Count > 0)
            {
                if (agent.navMeshAgent.remainingDistance < agent.config.trackNextDistance)
                {
                    // set new destination
                    agent.navMeshAgent.SetDestination(agent.mappedPlayerPositions[currentTrackIndex]);
                    agent.LookAt(agent.navMeshAgent.destination, agent.config.trackLookSpeed);
                    // iterate through destination list
                    if (currentTrackIndex < agent.mappedPlayerPositions.Count - 1) currentTrackIndex++;
                    // exit state if track fully mapped
                    else if (agent.trackMapped)
                    {
                        agent.EnemyStateMachine.ChangeState(EnemyStateId.Patrol);
                    }
                }
            }
        }
    }
}