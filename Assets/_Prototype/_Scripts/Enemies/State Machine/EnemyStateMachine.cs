using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemies
{
    public class EnemyStateMachine
    {
        public EnemyState[] states;
        public EnemyAgent agent;
    
        public EnemyStateId currentState;
    
        // constructor for EnemyStateMachine
        // get enum of all states; create states[] array
        public EnemyStateMachine(EnemyAgent agent)
        {
            this.agent = agent;
            int numStates = System.Enum.GetNames(typeof(EnemyStateId)).Length;
            states = new EnemyState[numStates];
        }
    
        public void RegisterState(EnemyState state)
        {
            int stateIndex = (int)state.GetId();
            states[stateIndex] = state;
        }
    
        public EnemyState GetState(EnemyStateId stateId)
        {
            int index = (int)stateId;
            return states[index];
        }
        
        public void ChangeState(EnemyStateId newState)
        {
            // throw debug message
            Debug.Log($"Changed state from {currentState} to {newState}");
            
            GetState(currentState)?.Exit(agent);
            currentState = newState;
            GetState(currentState)?.Enter(agent);
        }
        
        public void Update()
        {
            GetState(currentState)?.Update(agent);
        }
    }
}

