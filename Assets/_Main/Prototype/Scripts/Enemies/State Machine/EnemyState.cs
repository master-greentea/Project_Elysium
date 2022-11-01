using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemies
{
    public enum EnemyStateId
    {
        Idle, Chase, Patrol, Track, Intercept, Stare
    }
    
    // abstract class for EnemyState with simple state transition methods
    public interface EnemyState
    {
        EnemyStateId GetId();
        void Enter(EnemyAgent agent);
        void Update(EnemyAgent agent);
        void Exit(EnemyAgent agent);
    }
}
