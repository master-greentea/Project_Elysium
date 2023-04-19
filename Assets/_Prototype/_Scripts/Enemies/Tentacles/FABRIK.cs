using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FABRIK : MonoBehaviour
{
    [Header("IK Properties")]
    public Transform rootJoint;
    public Transform tipJoint;
    // public Transform effectorTarget;
    public Vector3 targetEffectorPoint;
    private Vector3 effectorPoint;
    public int iterations = 4;
    public float tolerance = 0.01f;

    [Header("Debug")]
    public bool debugJoints = true;
    public float gizmoSize = 1;

    private float[] jointLengths;
    private Transform[] jointTransforms;
    private Vector3[] jointPositions;

    void Awake()
    {
        var numberOfJoints = 0;
        var current = rootJoint;
        while (current != tipJoint)
        {
            numberOfJoints++;

            if (current.childCount > 0)
            {
                current = current.GetChild(0);
            }
            else
            {
                break;
            }
        }

        // Count the tip joint
        numberOfJoints++;

        jointLengths = new float[numberOfJoints];
        jointPositions = new Vector3[numberOfJoints];
        jointTransforms = new Transform[numberOfJoints];

        current = rootJoint;
        for (int i = 0; i < numberOfJoints; i++)
        {
            jointTransforms[i] = current;

            if (current.childCount > 0)
            {
                jointLengths[i] = (current.GetChild(0).position - current.position).magnitude;
                current = current.GetChild(0);
            }
            else
            {
                // The final joint in the chain has no length
                jointLengths[i] = 0;
                break;
            }
        }
    }

    void DoBackwardsIK()
    {
        for (int i = jointPositions.Length - 1; i >= 0; i--)
        {
            if (i == jointPositions.Length - 1)
            {
                jointPositions[i] = effectorPoint;
            }
            else
            {
                jointPositions[i] = jointPositions[i + 1] + (jointPositions[i] - jointPositions[i + 1]).normalized * jointLengths[i];
            }
        }
    }

    void DoForwardsIK()
    {
        for (int i = 0; i < jointPositions.Length; i++)
        {
            if (i == 0)
            {
                jointPositions[i] = rootJoint.position;
            }
            else
            {
                jointPositions[i] = jointPositions[i - 1] + (jointPositions[i] - jointPositions[i - 1]).normalized * jointLengths[i - 1];
            }
        }
    }

    float GetTipToEffectorDistance()
    {
        return Vector3.Distance(jointPositions[jointPositions.Length - 1], effectorPoint);
    }

    void Update()
    {
        for (int i = 0; i < jointTransforms.Length; i++)
        {
            jointPositions[i] = jointTransforms[i].position;
        }

        int numIterations = 0;
        float tipToEffectorDistance = GetTipToEffectorDistance();
        while (tipToEffectorDistance > tolerance)
        {
            DoBackwardsIK();
            DoForwardsIK();

            numIterations++;
            if (numIterations > iterations)
            {
                break;
            }
            else
            {
                tipToEffectorDistance = GetTipToEffectorDistance();
            }
        }

        for (int i = 0; i < jointPositions.Length - 1; i++)
        {
            jointTransforms[i].position = jointPositions[i];
            jointTransforms[i].rotation = Quaternion.LookRotation(jointPositions[i] - jointPositions[i + 1], jointTransforms[i].up);
        }
        
        MoveTowardsTarget();
        RandomTarget();
    }

    void MoveTowardsTarget()
    {
        effectorPoint = Vector3.Lerp(effectorPoint, targetEffectorPoint, Time.deltaTime * 5);
    }

    void RandomTarget()
    {
        if (Random.value < .01f)
            targetEffectorPoint = Random.insideUnitSphere * 2 + transform.position;
    }

    void OnDrawGizmos()
    {
        if (debugJoints && rootJoint != null && tipJoint != null)
        {
            var current = rootJoint;
            while (current != tipJoint)
            {
                if (current.childCount > 0)
                {
                    var child = current.GetChild(0);

                    var delta = (child.position - current.position).normalized;
                    var length = Vector3.Distance(current.position, child.position);

                    DebugStatics.DrawWireCapsule(current.position + delta * length / 2, Quaternion.FromToRotation(Vector3.up, delta), gizmoSize, length, Color.blue);

                    current = child;
                }
            }
        }
    }
}
