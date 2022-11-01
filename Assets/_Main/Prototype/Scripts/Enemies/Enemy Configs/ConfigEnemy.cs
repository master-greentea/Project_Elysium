using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemies
{
    // config asset for all enemy properties
    [CreateAssetMenu()]
    public class ConfigEnemy : ScriptableObject
    {
        [Header("Idle State")]
        public float restLookInterval;
        [HideInInspector] public float restTimer;
        [Range(0, 1)] public float restLookSpeed;
        public int maxRest;
        
        [Space(10)] [Header("Chase State")]
        public float chaseSpeed;
        public float chaseAccelerationDuration;
        [Space(10)]
        public float maxDistance;

        [Space(10)] [Header("Track State")]
        public float trackSpeed;
        public float trackAccelerationDuration;
        [Space(10)]
        [Tooltip("How often positions are mapped during tracking")] public float mapTrackInterval;
        [HideInInspector] public float trackTimer;
        [Tooltip("How many positions are mapped during tracking")] public int mapPositionCount;
        [Tooltip("Minimum distance to switch track to next destination")] public float trackNextDistance;
        [Range(0, 1)] public float trackLookSpeed;

        [Space(10)] [Header("Patrol State")]
        public float patrolSpeed;
        public float patrolAccelerationDuration;
        public int maxRoam;

        [Space(10)] [Header("Stare State")]
        public float checkSeenByPlayerInterval;
        [Tooltip ("How long before enemy turn to look at player")] public float delayBeforeTurnToPlayer;
        [Tooltip ("How long before enemy goes into tracking state")] public float delayBeforeTrackPlayer;
    }
}

