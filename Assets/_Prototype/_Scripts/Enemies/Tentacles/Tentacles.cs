using System;
using System.Collections;
using System.Collections.Generic;
using SplineMesh;
using UnityEngine;

[RequireComponent(typeof(Spline))]
public class Tentacles : MonoBehaviour
{
    private Vector3 lastPosition;
    [SerializeField] private float checkDistanceInterval;
    private float checkDistanceTimer;
    [SerializeField] private float deltaDistance;
    
    [Header("Attachable Points")]
    [SerializeField] private LayerMask attachableLayers;
    [SerializeField] private float attachableRadius;
    private List<Vector3> attachablePoints = new List<Vector3>();

    [Header("Tentacle Values")]
    public float startScale = 1;
    public float endScale = 1;
    public float startRoll = 0, endRoll = 0;
    private Spline spline { get => GetComponent<Spline>(); }

    private void OnValidate() {
        // apply scale and roll at each node
        float currentLength = 0;
        foreach (CubicBezierCurve curve in spline.GetCurves()) {
            float startRate = currentLength / spline.Length;
            currentLength += curve.Length;
            float endRate = currentLength / spline.Length;

            curve.n1.Scale = Vector2.one * (startScale + (endScale - startScale) * startRate);
            curve.n2.Scale = Vector2.one * (startScale + (endScale - startScale) * endRate);

            curve.n1.Roll = startRoll + (endRoll - startRoll) * startRate;
            curve.n2.Roll = startRoll + (endRoll - startRoll) * endRate;
        }
    }
    
    void Start()
    {
        lastPosition = transform.position;
    }
    
    void Update()
    {
        CheckDistance();
    }

    void CheckDistance()
    {
        // check based on time
        checkDistanceTimer -= Time.deltaTime;
        if (checkDistanceTimer > 0) return;
        checkDistanceTimer = checkDistanceInterval;
        // if moved
        // if ((transform.position - lastPosition).magnitude < deltaDistance) return;
        // lastPosition = transform.position;
        // do stuff
        GetPoints();
    }

    void GetPoints()
    {
        attachablePoints.Clear();
        foreach (var col in Physics.OverlapSphere(transform.position, attachableRadius, attachableLayers, QueryTriggerInteraction.Collide))
        {
            Vector3[] verts = col.GetComponent<MeshFilter>().mesh.vertices;
            foreach (var vert in verts)
            {
                Vector3 worldVert = col.transform.TransformPoint(vert);
                Physics.Raycast(transform.position, (worldVert - transform.position).normalized, out RaycastHit hit, attachableRadius, attachableLayers);
                // find valid points
                if (hit.point == worldVert )
                {
                    attachablePoints.Add(hit.point);
                    Vector3 localHitPoint = transform.InverseTransformPoint(hit.point);
                    spline.nodes[1].Position = localHitPoint;
                    spline.nodes[1].Direction = localHitPoint + new Vector3(0, .5f, 0);
                    Debug.DrawLine(transform.position, hit.point, Color.green, checkDistanceInterval - .01f);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attachableRadius);
    }
}
