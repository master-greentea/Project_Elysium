using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Enemies
{
    public class EnemyAgent : MonoBehaviour
    {
        public EnemyStateMachine EnemyStateMachine;
        public EnemyStateId initialState;
        [HideInInspector] public ConfigEnemy config;
        [HideInInspector] public EnemyFov enemyFov;
        [HideInInspector] public Transform playerTransform;
        [HideInInspector] public NavMeshAgent navMeshAgent;

        // tracking
        [HideInInspector] public List<Vector3> mappedPlayerPositions;
        [HideInInspector] public bool trackMapped;
        // timing values
        [HideInInspector] public float lerpElapsed;

        private void InitializeStateMachine()
        {
            EnemyStateMachine = new EnemyStateMachine(this);
            // register all states
            EnemyStateMachine.RegisterState(new IdleState());
            EnemyStateMachine.RegisterState(new ChaseState());
            EnemyStateMachine.RegisterState(new TrackState());
            EnemyStateMachine.RegisterState(new PatrolState());
            EnemyStateMachine.RegisterState(new StareState());
            EnemyStateMachine.RegisterState(new InterceptState());
            // change to initial state
            EnemyStateMachine.ChangeState(initialState);
        }
        private void AssignComponents()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            enemyFov = GetComponent<EnemyFov>();
        }
        private void Start()
        {
            InitializeStateMachine();
            AssignComponents();
        }
        
        private void Update()
        {
            EnemyStateMachine.Update();
            // Debug.Log("Current state: " + enemyStateMachine.currentState);
            // Debug.Log(IsSeenByPlayer());
        }

        // behavioral methods
        // common method to lerp self speed
        public void SpeedChange(float targetSpeed, float lerpDuration)
        {
            if (lerpElapsed < lerpDuration)
            {
                navMeshAgent.speed = Mathf.Lerp(navMeshAgent.speed, targetSpeed, lerpElapsed / lerpDuration);
                lerpElapsed += Time.deltaTime;
            }
        }
        
        // check if player is detected either by in sight or within minimum distance
        public bool IsPlayerDetected()
        {
            if (enemyFov.objectsDetected.Count > 0)
            {
                foreach (GameObject obj in enemyFov.objectsDetected)
                {
                    if (obj.CompareTag("Player"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // check if player currently looking at self
        public bool IsSeenByPlayer()
        {
            // Physics.OverlapSphereNonAlloc(transform.position, enemyFov.sightRange, new Collider[1], LayerMask.GetMask("Player")) > 0 &&
            return  !Physics.Linecast(transform.position, playerTransform.position, enemyFov.obstacleLayers)
                    && GetComponent<Renderer>().isVisible;
        }
        
        // map waypoints for tracking down player
        public void MapPlayerTrack()
        {
            if (mappedPlayerPositions.Count < config.mapPositionCount)
            {
                config.trackTimer -= Time.deltaTime;
                if (config.trackTimer < 0)
                {
                    config.trackTimer += config.mapTrackInterval;
                    mappedPlayerPositions.Add(playerTransform.position);
                }
            }
            else
            {
                // Debug.Log("Track Complete");
                trackMapped = true;
            }
        }

        // make self turn to target
        public void LookAt(Vector3 target, float rotationSpeed)
        {
            Vector3 directionToTarget = target - transform.position;
            directionToTarget.y = 0;
            Quaternion rotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
        }
        
        public Vector3 RandomWorldPos()
        {
            PatrolBounds patrolBounds = FindObjectOfType<PatrolBounds>();
            Vector3 min = patrolBounds.min.position;
            Vector3 max = patrolBounds.max.position;

            return new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y),
                Random.Range(min.z, max.z));
        }

        // debug gizmos
        private void OnDrawGizmos()
        {
            // mapped player track
            foreach (Vector3 t in mappedPlayerPositions)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(t, .5f);
            }
            // path line
            if (navMeshAgent)
            {
                Gizmos.color = Color.black;
                var agentPath = navMeshAgent.path;
                Vector3 previousCorner = transform.position;
                foreach (var corner in agentPath.corners)
                {
                    Gizmos.DrawLine(previousCorner, corner);
                    Gizmos.DrawSphere(corner, .1f);
                    previousCorner = corner;
                }
            }
        }
    }
}

