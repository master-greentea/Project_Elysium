using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// debug to draw enemy behavior gizmos
public class DebugNavMeshAgent : MonoBehaviour
{
    [SerializeField] private NavMeshAgent _navMeshAgent;
    public bool velocity;
    public bool desiredVelocity;

    void OnDrawGizmos()
    {
        if (velocity)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + _navMeshAgent.velocity);
        }

        if (desiredVelocity)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + _navMeshAgent.desiredVelocity);
        }
    }
}
