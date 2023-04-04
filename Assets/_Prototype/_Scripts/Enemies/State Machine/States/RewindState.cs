using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Enemies
{
    // track is immediately entered when enemy loses sight of player
    public class RewindState : EnemyState
    {
        public static Vector3 positionToRewindTo;
        private EnemyStateId stateToResetTo;
        private int rewindPositionIndex;
        // speed & acceleration
        private float rewindSpeed;
        private float rewindAcceleration;

        public EnemyStateId GetId()
        {
            return EnemyStateId.Rewind;
        }
    
        public void Enter(EnemyAgent agent)
        {
            // calculate position and reset state from rewind info
            rewindPositionIndex = RewindManager.enemyRewindInfoList.Count - RewindManager.SetRewindTime;
            stateToResetTo = RewindManager.enemyRewindInfoList[rewindPositionIndex].stateId;
            // set rewind speed & acceleration
            rewindSpeed = agent.config.chaseSpeed * RewindManager.RewindSpeed + (Services.PlayerController.walkSpeed - agent.config.chaseSpeed) * 2;
            rewindAcceleration = 50;
            agent.ChangeSpeed(rewindSpeed, rewindAcceleration);
            agent.navMeshAgent.angularSpeed = 1000;
        }
    
        public void Update(EnemyAgent agent)
        {
            agent.navMeshAgent.SetDestination(positionToRewindTo);
            if (!RewindManager.isRewinding) agent.EnemyStateMachine.ChangeState(stateToResetTo);
        }

        public void Exit(EnemyAgent agent)
        {
            // clean rewind info list
            agent.LookAt(agent.playerTransform.position, 100); // reset to look at player
            RewindManager.enemyRewindInfoList.RemoveRange(rewindPositionIndex, RewindManager.SetRewindTime);
            agent.navMeshAgent.angularSpeed = 260;
        }
    }
}