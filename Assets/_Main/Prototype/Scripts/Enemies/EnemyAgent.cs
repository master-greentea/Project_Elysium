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
        public ConfigEnemy config;
        [HideInInspector] public EnemyFov enemyFov;
        [HideInInspector] public Transform playerTransform;
        [HideInInspector] public NavMeshAgent navMeshAgent;

        // tracking
        [HideInInspector] public List<Vector3> mappedPlayerPositions;
        [HideInInspector] public bool trackMapped;

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

            // debugs
            // Debug.Log(IsSeenByPlayer());
            var distanceToPlayer = (playerTransform.position - transform.position).magnitude;
            if (distanceToPlayer < 2 && !GameManager.isGameEnded) Services.GameManager.EndGame();
        }

        // behavioral methods
        /// <summary>
        /// Smoothly change self movement speed.
        /// </summary>
        /// <param name="targetSpeed">Speed to change to</param>
        /// <param name="lerpDuration">How long does it take to change to that speed</param>
        public void ChangeSpeed(float targetSpeed, float lerpDuration)
        {
            if (activeSpeedChangeRoutine != null) StopCoroutine(activeSpeedChangeRoutine);
            activeSpeedChangeRoutine = StartCoroutine(SpeedChangeRoutine(targetSpeed, lerpDuration));
        }
        // speed change coroutine
        private Coroutine activeSpeedChangeRoutine;
        private IEnumerator SpeedChangeRoutine(float targetSpeed, float lerpDuration)
        {
            var lerpElapsed = 0f;
            while (lerpElapsed < lerpDuration)
            {
                navMeshAgent.speed = Mathf.Lerp(navMeshAgent.speed, targetSpeed, lerpElapsed / lerpDuration);
                lerpElapsed += Time.deltaTime;
                yield return null;
            }
        }
        
        /// <summary>
        /// check if player is detected either by in sight or within minimum distance
        /// </summary>
        /// <returns>if player is detected either by in sight or within minimum distance</returns>
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
        
        /// <summary>
        /// check if player currently looking at self
        /// </summary>
        /// <returns>if player currently looking at self</returns>
        public bool IsSeenByPlayer()
        {
            // Physics.OverlapSphereNonAlloc(transform.position, enemyFov.sightRange, new Collider[1], LayerMask.GetMask("Player")) > 0 &&
            return  !Physics.Linecast(transform.position, playerTransform.position, enemyFov.obstacleLayers)
                    && GetComponent<Renderer>().isVisible;
        }
        
        /// <summary>
        /// Map waypoints to track player
        /// </summary>
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

        /// <summary>
        /// Orient self to target position
        /// </summary>
        /// <param name="target">Position to look at</param>
        /// <param name="rotationSpeed">Speed of rotation</param>
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

        /// <summary>
        /// Debug gizmos for enemy vision.
        /// </summary>
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

