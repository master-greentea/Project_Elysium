using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Enemies
{
    public class EnemyAgent : MonoBehaviour
    {
        public EnemyStateMachine enemyStateMachine;
        public EnemyStateId initialState;
        [HideInInspector] public ConfigEnemy config;
        [HideInInspector] public EnemyFov enemyFov;
        [HideInInspector] public Transform _playerTransform;
        [HideInInspector] public NavMeshAgent _navMeshAgent;
        
        // tracking
        [HideInInspector] public List<Vector3> mappedPlayerPositions;
        [HideInInspector] public bool trackMapped;
        
        // values
        [HideInInspector] public float lerpElapsed;

        void InitializeStateMachine()
        {
            enemyStateMachine = new EnemyStateMachine(this);
            // register all states
            enemyStateMachine.RegisterState(new IdleState());
            enemyStateMachine.RegisterState(new ChaseState());
            enemyStateMachine.RegisterState(new TrackState());
            enemyStateMachine.RegisterState(new PatrolState());
            // change to initial state
            enemyStateMachine.ChangeState(initialState);
        }
        
        void AssignComponents()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            enemyFov = GetComponent<EnemyFov>();
        }
        
        void Start()
        {
            InitializeStateMachine();
            AssignComponents();
        }
        
        void Update()
        {
            enemyStateMachine.Update();
            // Debug.Log("Current state: " + enemyStateMachine.currentState);

            // Vector3 dir = _navMeshAgent.destination - transform.position;
            // dir.y = 0;
            // Quaternion rot = Quaternion.LookRotation(dir);
            // transform.rotation = Quaternion.Lerp(transform.rotation, rot, 100 * Time.deltaTime);
        }

        // behavioral methods
        public void SpeedChange(float targetSpeed, float lerpDuration)
        {
            if (lerpElapsed < lerpDuration)
            {
                _navMeshAgent.speed = Mathf.Lerp(_navMeshAgent.speed, targetSpeed, lerpElapsed / lerpDuration);
                lerpElapsed += Time.deltaTime;
            }
        }
        
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
        
        public void MapPlayerTrack()
        {
            if (mappedPlayerPositions.Count < config.mapPositionCount)
            {
                config.trackTimer -= Time.deltaTime;
                if (config.trackTimer < 0)
                {
                    config.trackTimer += config.mapTrackInterval;
                    mappedPlayerPositions.Add(_playerTransform.position);
                }
            }
            else
            {
                Debug.Log("Track Complete");
                trackMapped = true;
            }
        }

        public void LookAt(Vector3 target, float rotationSpeed)
        {
            Vector3 directionToTarget = target - transform.position;
            directionToTarget.y = 0;
            Quaternion rotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
        }
        
        public Vector3 RandomWorldPos()
        {
            PatrolBounds patrolBounds = GameObject.FindObjectOfType<PatrolBounds>();
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
            Gizmos.color = Color.black;
            var agentPath = _navMeshAgent.path;
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

