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
            rewindPositionIndex = RewindManager.EnemyRewindInfoList.Count - RewindManager.setRewindTime;
            stateToResetTo = RewindManager.EnemyRewindInfoList[rewindPositionIndex].stateId;
            // set rewind speed & acceleration
            rewindSpeed = agent.config.chaseSpeed * RewindManager.RewindSpeed;
            rewindAcceleration = 50;
            agent.ChangeSpeed(rewindSpeed, rewindAcceleration);
            agent.navMeshAgent.angularSpeed = 1000;
            Debug.Log("Entered rewind state, target position: " + positionToRewindTo);
        }
    
        public void Update(EnemyAgent agent)
        {
            agent.navMeshAgent.Move((positionToRewindTo - agent.transform.position).normalized * (rewindSpeed * Time.fixedDeltaTime));
            if (!RewindManager.isRewinding) agent.EnemyStateMachine.ChangeState(stateToResetTo);
        }

        public void Exit(EnemyAgent agent)
        {
            // clean rewind info list
            RewindManager.EnemyRewindInfoList.RemoveRange(rewindPositionIndex, RewindManager.setRewindTime);
            agent.navMeshAgent.angularSpeed = 260;
        }
    }
}